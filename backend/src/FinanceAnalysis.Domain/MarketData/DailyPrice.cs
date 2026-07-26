using FinanceAnalysis.Domain.Catalog;

namespace FinanceAnalysis.Domain.MarketData;

/// <summary>
/// One trading day's OHLCV bar for one symbol.
/// </summary>
/// <remarks>
/// This table is append-only by design. There are no mutators and the persistence layer
/// inserts with <c>ON CONFLICT DO NOTHING</c> against the unique
/// <c>(stock_id, trade_date)</c> index, so re-running an ingestion for a day that was
/// already collected is a no-op rather than an overwrite. History is never revised.
/// </remarks>
public sealed class DailyPrice
{
    private DailyPrice()
    {
    }

    public DailyPrice(
        int stockId,
        DateOnly tradeDate,
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        long volume,
        int dataSourceId,
        decimal? volumeWeightedAveragePrice = null,
        int? transactionCount = null)
    {
        StockId = stockId;
        TradeDate = tradeDate;
        Open = open;
        High = high;
        Low = low;
        Close = close;
        Volume = volume;
        DataSourceId = dataSourceId;
        VolumeWeightedAveragePrice = volumeWeightedAveragePrice;
        TransactionCount = transactionCount;
        IngestedAt = DateTimeOffset.UtcNow;
    }

    public long Id { get; private set; }

    public int StockId { get; private set; }

    public DateOnly TradeDate { get; private set; }

    public decimal Open { get; private set; }

    public decimal High { get; private set; }

    public decimal Low { get; private set; }

    public decimal Close { get; private set; }

    public long Volume { get; private set; }

    public decimal? VolumeWeightedAveragePrice { get; private set; }

    public int? TransactionCount { get; private set; }

    public int DataSourceId { get; private set; }

    public DateTimeOffset IngestedAt { get; private set; }

    public Stock Stock { get; private set; } = null!;

    public DataSource DataSource { get; private set; } = null!;
}
