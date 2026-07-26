using FinanceAnalysis.Application.Abstractions.Persistence.Projections;

namespace FinanceAnalysis.Application.Abstractions.Persistence;

/// <summary>
/// Aggregate read queries that span stocks, sectors and prices. Kept apart from the
/// aggregate repositories because these are reporting projections, not entity access.
/// </summary>
public interface IMarketOverviewRepository
{
    Task<MarketBreadth> GetBreadthAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SectorPerformance>> GetSectorPerformanceAsync(CancellationToken cancellationToken = default);

    /// <summary>Largest one-day percentage gains, most positive first.</summary>
    Task<IReadOnlyList<StockSummary>> GetTopGainersAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>Largest one-day percentage losses, most negative first.</summary>
    Task<IReadOnlyList<StockSummary>> GetTopLosersAsync(int count, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockSummary>> GetMostActiveAsync(int count, CancellationToken cancellationToken = default);
}
