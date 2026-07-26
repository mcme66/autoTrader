namespace FinanceAnalysis.Application.Abstractions.Persistence.Projections;

/// <summary>
/// Aggregate one-day performance for a sector, averaged across its tracked symbols.
/// </summary>
public sealed record SectorPerformance(
    string SectorKey,
    string SectorName,
    int StockCount,
    decimal? AverageChangePercent,
    long TotalVolume);
