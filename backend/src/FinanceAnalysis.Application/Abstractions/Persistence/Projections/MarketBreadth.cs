namespace FinanceAnalysis.Application.Abstractions.Persistence.Projections;

/// <summary>
/// Counts of advancing, declining and unchanged symbols for the most recent trading day.
/// </summary>
public sealed record MarketBreadth(
    DateOnly? TradeDate,
    int Advancers,
    int Decliners,
    int Unchanged,
    long TotalVolume);
