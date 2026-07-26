namespace FinanceAnalysis.Application.Abstractions.MarketData;

/// <summary>
/// A provider-agnostic OHLCV bar. Providers translate their own wire formats into this shape,
/// which is the only thing the ingestion pipeline understands.
/// </summary>
public sealed record DailyBar(
    string Symbol,
    DateOnly TradeDate,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    long Volume,
    decimal? VolumeWeightedAveragePrice = null,
    int? TransactionCount = null)
{
    /// <summary>
    /// Rejects bars that cannot be real, so bad provider data is dropped at the boundary
    /// rather than failing a database check constraint mid-batch.
    /// </summary>
    public bool IsValid() =>
        Open > 0
        && High > 0
        && Low > 0
        && Close > 0
        && High >= Low
        && Volume >= 0
        && !string.IsNullOrWhiteSpace(Symbol);
}
