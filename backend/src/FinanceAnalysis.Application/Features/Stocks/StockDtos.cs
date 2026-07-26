namespace FinanceAnalysis.Application.Features.Stocks;

/// <summary>A stock as it appears in lists, search results and tables.</summary>
public sealed record StockDto(
    string Symbol,
    string CompanyName,
    string? SectorKey,
    string? SectorName,
    string? IndustryName,
    string? Exchange,
    string CurrencyCode,
    bool IsTracked,
    DateOnly? LatestTradeDate,
    decimal? LatestClose,
    decimal? PreviousClose,
    decimal? ChangeAmount,
    decimal? ChangePercent,
    long? LatestVolume);

/// <summary>Everything the stock detail page needs that is not price history.</summary>
public sealed record StockDetailDto(
    StockDto Summary,
    string? Description,
    string? HomepageUrl,
    string? CountryCode,
    int? EmployeeCount,
    DateOnly? ListedOn,
    DateOnly? DelistedOn,
    PriceStatisticsDto? Statistics);

/// <summary>Descriptive statistics derived from the stock's stored history.</summary>
public sealed record PriceStatisticsDto(
    int BarCount,
    DateOnly FirstTradeDate,
    DateOnly LastTradeDate,
    decimal PeriodHigh,
    decimal PeriodLow,
    decimal AverageClose,
    long AverageVolume,
    decimal? PeriodChangePercent);

/// <summary>One OHLCV bar.</summary>
public sealed record PriceBarDto(
    DateOnly TradeDate,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    long Volume,
    decimal? VolumeWeightedAveragePrice);

/// <summary>A symbol's price history over a requested window.</summary>
public sealed record PriceHistoryDto(
    string Symbol,
    DateOnly From,
    DateOnly To,
    IReadOnlyList<PriceBarDto> Bars,
    PriceStatisticsDto? Statistics);

/// <summary>Request to start or stop collecting prices for a symbol.</summary>
public sealed record UpdateTrackingRequest(bool IsTracked);
