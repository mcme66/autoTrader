using FinanceAnalysis.Application.Abstractions.Persistence.Projections;
using FinanceAnalysis.Domain.MarketData;

namespace FinanceAnalysis.Application.Features.Stocks;

/// <summary>
/// Projection-to-DTO mapping. Kept in one place so the wire contract cannot drift between the
/// several services that return stock shapes.
/// </summary>
internal static class StockMappings
{
    public static StockDto ToDto(this StockSummary summary) => new(
        summary.Symbol,
        summary.CompanyName,
        summary.SectorKey,
        summary.SectorName,
        summary.IndustryName,
        summary.Exchange,
        summary.CurrencyCode,
        summary.IsTracked,
        summary.LatestTradeDate,
        summary.LatestClose,
        summary.PreviousClose,
        summary.ChangeAmount,
        summary.ChangePercent,
        summary.LatestVolume);

    public static PriceBarDto ToDto(this DailyPrice price) => new(
        price.TradeDate,
        price.Open,
        price.High,
        price.Low,
        price.Close,
        price.Volume,
        price.VolumeWeightedAveragePrice);

    /// <summary>
    /// Summarises a run of bars. Returns null for an empty series rather than a row of zeroes,
    /// so the UI can distinguish "no data yet" from "a flat market".
    /// </summary>
    public static PriceStatisticsDto? ToStatistics(this IReadOnlyList<DailyPrice> bars)
    {
        if (bars.Count == 0)
        {
            return null;
        }

        var first = bars[0];
        var last = bars[^1];

        return new PriceStatisticsDto(
            bars.Count,
            first.TradeDate,
            last.TradeDate,
            bars.Max(b => b.High),
            bars.Min(b => b.Low),
            Math.Round(bars.Average(b => b.Close), 4),
            (long)bars.Average(b => b.Volume),
            first.Close == 0m ? null : Math.Round((last.Close - first.Close) / first.Close * 100m, 4));
    }
}
