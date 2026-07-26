namespace FinanceAnalysis.Application.Abstractions.Persistence.Projections;

/// <summary>
/// A portfolio holding joined to its symbol and latest close, ready for valuation maths.
/// </summary>
public sealed record HoldingValuation(
    Guid HoldingId,
    int StockId,
    string Symbol,
    string CompanyName,
    string? SectorKey,
    string? SectorName,
    decimal Quantity,
    decimal AverageCost,
    DateOnly? OpenedOn,
    string? Notes,
    decimal? LatestClose,
    decimal? PreviousClose,
    DateOnly? LatestTradeDate);
