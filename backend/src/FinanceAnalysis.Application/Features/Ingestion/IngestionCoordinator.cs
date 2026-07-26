using FinanceAnalysis.Application.Abstractions.Ingestion;
using FinanceAnalysis.Application.Abstractions.MarketData;
using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Application.Configuration;
using FinanceAnalysis.Domain.Enums;
using FinanceAnalysis.Domain.Exceptions;
using FinanceAnalysis.Domain.MarketData;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceAnalysis.Application.Features.Ingestion;

public sealed class IngestionCoordinator(
    IIngestionJobQueue queue,
    IIngestionRunRepository runs,
    IDataSourceRepository dataSources,
    IMarketDataProviderResolver providers,
    IUnitOfWork unitOfWork,
    IClock clock,
    IOptions<IngestionOptions> ingestionOptions,
    IOptions<MarketDataOptions> marketDataOptions,
    ILogger<IngestionCoordinator> logger) : IIngestionCoordinator
{
    private readonly IngestionOptions _ingestion = ingestionOptions.Value;
    private readonly MarketDataOptions _marketData = marketDataOptions.Value;

    public async Task<IngestionRunDto> EnqueueDailyPricesAsync(
        DateOnly? tradeDate,
        CancellationToken cancellationToken = default)
    {
        var provider = providers.Resolve();
        var dataSource = await GetDataSourceAsync(provider, cancellationToken).ConfigureAwait(false);
        var targetDate = tradeDate ?? MostRecentLikelyTradingDay();

        var run = IngestionRun.ForTradingDay(IngestionRunType.DailyPrices, dataSource.Id, targetDate);
        runs.Add(run);

        if (_ingestion.SkipAlreadyIngestedDays
            && await runs.HasSucceededForDateAsync(IngestionRunType.DailyPrices, targetDate, cancellationToken)
                .ConfigureAwait(false))
        {
            run.Skip($"{targetDate:yyyy-MM-dd} was already ingested successfully.");
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            logger.LogInformation(
                "Daily ingestion for {TradeDate} skipped; the day was already collected. Run {RunId}.",
                targetDate,
                run.Id);

            return run.ToDto(dataSource.Key);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await EnqueueAsync(
            new IngestionJob(
                run.Id,
                IngestionRunType.DailyPrices,
                provider.Key,
                dataSource.Id,
                TradeDate: targetDate),
            run,
            dataSource.Key,
            cancellationToken).ConfigureAwait(false);

        return run.ToDto(dataSource.Key);
    }

    public async Task<IngestionRunDto> EnqueueBackfillAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        if (fromDate > toDate)
        {
            throw new BusinessRuleException("The backfill start date must not be after the end date.");
        }

        var spanDays = toDate.DayNumber - fromDate.DayNumber + 1;
        if (spanDays > _marketData.MaxBackfillDays)
        {
            throw new BusinessRuleException(
                $"A backfill may span at most {_marketData.MaxBackfillDays} days; {spanDays} were requested.");
        }

        var provider = providers.Resolve();
        var dataSource = await GetDataSourceAsync(provider, cancellationToken).ConfigureAwait(false);

        var run = IngestionRun.ForRange(dataSource.Id, fromDate, toDate);
        runs.Add(run);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await EnqueueAsync(
            new IngestionJob(
                run.Id,
                IngestionRunType.HistoricalBackfill,
                provider.Key,
                dataSource.Id,
                RangeStart: fromDate,
                RangeEnd: toDate),
            run,
            dataSource.Key,
            cancellationToken).ConfigureAwait(false);

        return run.ToDto(dataSource.Key);
    }

    public async Task<IngestionRunDto> GetRunAsync(Guid runId, CancellationToken cancellationToken = default)
    {
        var run = await runs.FindByIdAsync(runId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException("Ingestion run", runId);

        return run.ToDto(run.DataSource.Key);
    }

    public async Task<PagedResult<IngestionRunDto>> GetRunsAsync(
        PageRequest page,
        IngestionRunType? runType = null,
        IngestionRunStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var results = await runs.SearchAsync(page, runType, status, cancellationToken).ConfigureAwait(false);
        return results.Map(r => r.ToDto(r.DataSource.Key));
    }

    /// <summary>
    /// Best guess at the last day the market traded, used when the cron job posts without a
    /// date. Weekends are excluded here; public holidays are not, because the provider reports
    /// them and the run is then recorded as skipped rather than failed.
    /// </summary>
    private DateOnly MostRecentLikelyTradingDay()
    {
        var candidate = clock.UtcToday.AddDays(-1);

        for (var i = 0; i < _ingestion.MaxLookbackDays; i++)
        {
            if (candidate.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
            {
                return candidate;
            }

            candidate = candidate.AddDays(-1);
        }

        return candidate;
    }

    private Task<DataSource> GetDataSourceAsync(IMarketDataProvider provider, CancellationToken cancellationToken) =>
        dataSources.GetOrCreateAsync(provider.Key, provider.DisplayName, cancellationToken);

    /// <summary>
    /// Pushes the job onto the queue, marking the run failed if the queue is saturated. The
    /// run row is written either way so a rejected trigger is still visible to an operator.
    /// </summary>
    private async Task EnqueueAsync(
        IngestionJob job,
        IngestionRun run,
        string dataSourceKey,
        CancellationToken cancellationToken)
    {
        if (queue.TryEnqueue(job))
        {
            logger.LogInformation(
                "Queued {RunType} ingestion run {RunId} against provider {Provider}.",
                job.RunType,
                run.Id,
                dataSourceKey);
            return;
        }

        run.Fail("The ingestion queue is full; the previous run has not finished yet.");
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        throw new BusinessRuleException(
            "Ingestion is already running and the queue is full. Retry once the current run completes.");
    }
}
