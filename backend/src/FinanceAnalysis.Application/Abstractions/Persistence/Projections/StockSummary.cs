namespace FinanceAnalysis.Application.Abstractions.Persistence.Projections;

/// <summary>
/// Flattened stock row with its latest close and one-day change, used by list and search
/// screens. Projected in SQL so listing 300 symbols does not load 300 graphs into memory.
/// </summary>
public sealed record StockSummary(
    int StockId,
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
    long? LatestVolume)
{
    public decimal? ChangeAmount =>
        LatestClose is null || PreviousClose is null ? null : LatestClose - PreviousClose;

    public decimal? ChangePercent =>
        LatestClose is null || PreviousClose is null || PreviousClose == 0m
            ? null
            : Math.Round((LatestClose.Value - PreviousClose.Value) / PreviousClose.Value * 100m, 4);
}
