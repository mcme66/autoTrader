using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Application.Features.Stocks;

namespace FinanceAnalysis.Application.Features.MarketOverview;

/// <summary>Advance/decline counts for the most recent trading day held in the database.</summary>
public sealed record MarketBreadthDto(
    DateOnly? TradeDate,
    int Advancers,
    int Decliners,
    int Unchanged,
    long TotalVolume);

/// <summary>One-day performance of a sector, averaged across its tracked symbols.</summary>
public sealed record SectorPerformanceDto(
    string SectorKey,
    string SectorName,
    int StockCount,
    decimal? AverageChangePercent,
    long TotalVolume);

/// <summary>Everything the market overview page renders, in one response.</summary>
public sealed record MarketOverviewDto(
    MarketBreadthDto Breadth,
    IReadOnlyList<SectorPerformanceDto> Sectors,
    IReadOnlyList<StockDto> TopGainers,
    IReadOnlyList<StockDto> TopLosers,
    IReadOnlyList<StockDto> MostActive,
    int TrackedSymbolCount);

public interface IMarketOverviewService
{
    Task<MarketOverviewDto> GetAsync(int moversCount = 5, CancellationToken cancellationToken = default);
}

/// <summary>
/// Composes the market overview.
/// </summary>
/// <remarks>
/// The five queries are independent reads, but they are issued sequentially rather than in
/// parallel because a single <c>DbContext</c> is not thread-safe. At this data volume the whole
/// composition is a handful of milliseconds; if it ever stops being, the fix is a materialised
/// view refreshed by the ingestion job, not concurrent contexts.
/// </remarks>
public sealed class MarketOverviewService(
    IMarketOverviewRepository overview,
    IStockRepository stocks) : IMarketOverviewService
{
    private const int MaxMoversCount = 25;

    public async Task<MarketOverviewDto> GetAsync(
        int moversCount = 5,
        CancellationToken cancellationToken = default)
    {
        var count = Math.Clamp(moversCount, 1, MaxMoversCount);

        var breadth = await overview.GetBreadthAsync(cancellationToken).ConfigureAwait(false);
        var sectors = await overview.GetSectorPerformanceAsync(cancellationToken).ConfigureAwait(false);
        var gainers = await overview.GetTopGainersAsync(count, cancellationToken).ConfigureAwait(false);
        var losers = await overview.GetTopLosersAsync(count, cancellationToken).ConfigureAwait(false);
        var active = await overview.GetMostActiveAsync(count, cancellationToken).ConfigureAwait(false);
        var trackedCount = await stocks.CountTrackedAsync(cancellationToken).ConfigureAwait(false);

        return new MarketOverviewDto(
            new MarketBreadthDto(
                breadth.TradeDate,
                breadth.Advancers,
                breadth.Decliners,
                breadth.Unchanged,
                breadth.TotalVolume),
            [
                .. sectors.Select(s => new SectorPerformanceDto(
                    s.SectorKey,
                    s.SectorName,
                    s.StockCount,
                    s.AverageChangePercent,
                    s.TotalVolume)),
            ],
            [.. gainers.Select(StockMappings.ToDto)],
            [.. losers.Select(StockMappings.ToDto)],
            [.. active.Select(StockMappings.ToDto)],
            trackedCount);
    }
}
