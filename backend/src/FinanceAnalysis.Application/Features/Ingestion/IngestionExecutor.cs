using FinanceAnalysis.Application.Abstractions.Ingestion;
using FinanceAnalysis.Application.Abstractions.MarketData;
using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Domain.Enums;
using FinanceAnalysis.Domain.MarketData;

using Microsoft.Extensions.Logging;

namespace FinanceAnalysis.Application.Features.Ingestion;

/// <summary>
/// Collects bars from the provider and appends them to <c>daily_prices</c>.
/// </summary>
/// <remarks>
/// Every path through this class ends with the run row in a terminal state, because the
/// audit table is the only visibility an operator has into work that happened after the
/// HTTP response was already sent.
/// </remarks>
public sealed class IngestionExecutor(
    IIngestionRunRepository runs,
    IStockRepository stocks,
    IDailyPriceRepository prices,
    IMarketDataProviderResolver providers,
    IUnitOfWork unitOfWork,
    ILogger<IngestionExecutor> logger) : IIngestionExecutor
{
    public async Task ExecuteAsync(IngestionJob job, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(job);

        var run = await runs.FindByIdAsync(job.RunId, cancellationToken).ConfigureAwait(false);

        if (run is null)
        {
            logger.LogError("Ingestion run {RunId} was queued but no longer exists; the job was dropped.", job.RunId);
            return;
        }

        var tracked = await stocks.GetTrackedSymbolIdsAsync(cancellationToken).ConfigureAwait(false);

        if (tracked.Count == 0)
        {
            run.Skip("No symbols are currently tracked. Run a universe sync first.");
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            logger.LogWarning("Ingestion run {RunId} skipped because the tracked universe is empty.", run.Id);
            return;
        }

        run.Start(tracked.Count);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var provider = providers.Resolve(job.ProviderKey);
            var dates = ResolveDates(job);
            var closedDays = 0;

            foreach (var date in dates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var traded = await IngestDayAsync(run, provider, tracked, date, job.DataSourceId, cancellationToken)
                    .ConfigureAwait(false);

                if (!traded)
                {
                    closedDays++;
                }

                // Persisting after each day means a backfill interrupted at day 200 keeps the
                // first 199 days of progress and an accurate audit record.
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            if (closedDays == dates.Count)
            {
                run.Skip(dates.Count == 1
                    ? $"{dates[0]:yyyy-MM-dd} was not a trading day."
                    : "No trading days fell within the requested range.");
            }
            else
            {
                run.Succeed();
            }

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            logger.LogInformation(
                "Ingestion run {RunId} finished with status {Status}: {Received}/{Requested} symbols, "
                + "{Inserted} rows inserted, {Skipped} duplicates ignored.",
                run.Id,
                run.Status,
                run.SymbolsReceived,
                run.SymbolsRequested,
                run.RecordsInserted,
                run.RecordsSkipped);
        }
        catch (OperationCanceledException)
        {
            // Shutdown, not a data problem. Record it so the run is not left Running forever.
            run.Fail("The ingestion run was cancelled, most likely because the application is shutting down.");
            await SaveTerminalStateAsync(run, cancellationToken).ConfigureAwait(false);
            throw;
        }
#pragma warning disable CA1031 // A background worker must never let one bad run take the process down.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            logger.LogError(ex, "Ingestion run {RunId} failed.", run.Id);
            run.Fail(ex.Message);
            await SaveTerminalStateAsync(run, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Collects one trading day. Returns false when the provider reports the market was
    /// closed, which is an expected outcome for weekend and holiday triggers.
    /// </summary>
    private async Task<bool> IngestDayAsync(
        IngestionRun run,
        IMarketDataProvider provider,
        IReadOnlyDictionary<string, int> tracked,
        DateOnly date,
        int dataSourceId,
        CancellationToken cancellationToken)
    {
        var symbols = tracked.Keys.ToHashSet(StringComparer.Ordinal);
        var batch = await provider.GetDailyBarsAsync(date, symbols, cancellationToken).ConfigureAwait(false);

        if (!batch.IsTradingDay)
        {
            logger.LogDebug("Provider {Provider} reported {TradeDate} as a non-trading day.", provider.Key, date);
            return false;
        }

        var toInsert = new List<DailyPrice>(batch.Bars.Count);
        var rejected = 0;

        foreach (var bar in batch.Bars)
        {
            if (!bar.IsValid() || !tracked.TryGetValue(bar.Symbol, out var stockId))
            {
                rejected++;
                continue;
            }

            toInsert.Add(new DailyPrice(
                stockId,
                bar.TradeDate,
                bar.Open,
                bar.High,
                bar.Low,
                bar.Close,
                bar.Volume,
                dataSourceId,
                bar.VolumeWeightedAveragePrice,
                bar.TransactionCount));
        }

        if (rejected > 0)
        {
            logger.LogWarning(
                "Discarded {Rejected} bars for {TradeDate} from {Provider}: untracked symbol or failed validation.",
                rejected,
                date,
                provider.Key);
        }

        var result = await prices.InsertIgnoringDuplicatesAsync(toInsert, cancellationToken).ConfigureAwait(false);
        run.RecordProgress(toInsert.Count, result.Inserted, result.Skipped);

        return true;
    }

    /// <summary>
    /// Weekends are filtered here so a long backfill does not spend rate-limit budget on days
    /// the market was definitely closed. Holidays still cost one call and come back empty.
    /// </summary>
    private static List<DateOnly> ResolveDates(IngestionJob job)
    {
        if (job.RunType != IngestionRunType.HistoricalBackfill)
        {
            return job.TradeDate is null ? [] : [job.TradeDate.Value];
        }

        var start = job.RangeStart ?? throw new InvalidOperationException("A backfill job requires a start date.");
        var end = job.RangeEnd ?? throw new InvalidOperationException("A backfill job requires an end date.");

        var dates = new List<DateOnly>(end.DayNumber - start.DayNumber + 1);

        for (var date = start; date <= end; date = date.AddDays(1))
        {
            if (date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
            {
                dates.Add(date);
            }
        }

        return dates;
    }

    /// <summary>
    /// Writes the failure state with a fresh token, since the original may already be
    /// cancelled and losing the reason a run failed is worse than a slightly delayed shutdown.
    /// </summary>
    private async Task SaveTerminalStateAsync(IngestionRun run, CancellationToken cancellationToken)
    {
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await unitOfWork.SaveChangesAsync(
                cancellationToken.IsCancellationRequested ? timeout.Token : cancellationToken)
                .ConfigureAwait(false);
        }
#pragma warning disable CA1031 // Nothing useful remains to be done if the audit write itself fails.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            logger.LogError(ex, "Failed to record the terminal state of ingestion run {RunId}.", run.Id);
        }
    }
}
