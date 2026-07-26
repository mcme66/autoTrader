using FinanceAnalysis.Domain.MarketData;

namespace FinanceAnalysis.Application.Abstractions.Persistence;

public interface IDailyPriceRepository
{
    Task<IReadOnlyList<DailyPrice>> GetRangeAsync(
        int stockId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);

    Task<DailyPrice?> GetLatestAsync(int stockId, CancellationToken cancellationToken = default);

    Task<DateOnly?> GetLatestTradeDateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts bars, ignoring any that would collide with an existing
    /// <c>(stock_id, trade_date)</c> row, and reports how many were actually written.
    /// This is what makes the daily ingestion endpoint safe to trigger more than once and
    /// what guarantees history is never overwritten.
    /// </summary>
    Task<PriceInsertResult> InsertIgnoringDuplicatesAsync(
        IReadOnlyCollection<DailyPrice> prices,
        CancellationToken cancellationToken = default);
}

/// <summary>Outcome of an append-only bulk insert.</summary>
public readonly record struct PriceInsertResult(int Inserted, int Skipped);
