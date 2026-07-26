using FinanceAnalysis.Application.Common;

namespace FinanceAnalysis.Application.Abstractions.Persistence.Queries;

/// <summary>Ordering options for a stock search.</summary>
public enum StockSortOrder
{
    Symbol = 0,
    CompanyName = 1,
    Sector = 2,
}

/// <summary>
/// Normalized filter for the stock catalogue. Built by the service layer from request
/// parameters so the repository never sees raw user input.
/// </summary>
public sealed record StockSearchCriteria(
    PageRequest Page,
    string? SearchTerm = null,
    string? SectorKey = null,
    bool TrackedOnly = true,
    StockSortOrder SortBy = StockSortOrder.Symbol,
    bool Descending = false);
