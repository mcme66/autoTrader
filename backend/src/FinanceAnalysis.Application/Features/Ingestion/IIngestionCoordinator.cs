using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Domain.Enums;

namespace FinanceAnalysis.Application.Features.Ingestion;

/// <summary>
/// Accepts ingestion triggers, records them, and queues the work.
/// </summary>
/// <remarks>
/// This is the request-side half of ingestion; <see cref="IIngestionExecutor"/> is the
/// worker-side half. Keeping them apart means an HTTP trigger never holds a database
/// connection open for the duration of a multi-minute backfill.
/// </remarks>
public interface IIngestionCoordinator
{
    /// <summary>
    /// Queues collection of one trading day. When <paramref name="tradeDate"/> is null the
    /// most recent likely trading day is used, so the cron job needs no date arithmetic.
    /// </summary>
    Task<IngestionRunDto> EnqueueDailyPricesAsync(
        DateOnly? tradeDate,
        CancellationToken cancellationToken = default);

    /// <summary>Queues a historical load spanning an inclusive date range.</summary>
    Task<IngestionRunDto> EnqueueBackfillAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);

    Task<IngestionRunDto> GetRunAsync(Guid runId, CancellationToken cancellationToken = default);

    Task<PagedResult<IngestionRunDto>> GetRunsAsync(
        PageRequest page,
        IngestionRunType? runType = null,
        IngestionRunStatus? status = null,
        CancellationToken cancellationToken = default);
}
