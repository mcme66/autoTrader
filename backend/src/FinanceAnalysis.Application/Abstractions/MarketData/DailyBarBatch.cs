namespace FinanceAnalysis.Application.Abstractions.MarketData;

/// <summary>
/// The result of asking a provider for one trading day.
/// </summary>
/// <param name="TradeDate">The day requested.</param>
/// <param name="Bars">Bars for the requested symbols that the provider returned.</param>
/// <param name="IsTradingDay">
/// False when the provider reports the market was closed. The ingestion pipeline records a
/// skipped run rather than a failure, so weekend cron triggers do not raise alerts.
/// </param>
public sealed record DailyBarBatch(
    DateOnly TradeDate,
    IReadOnlyList<DailyBar> Bars,
    bool IsTradingDay = true)
{
    public static DailyBarBatch MarketClosed(DateOnly tradeDate) => new(tradeDate, [], IsTradingDay: false);
}
