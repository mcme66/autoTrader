namespace FinanceAnalysis.Application.Abstractions.MarketData;

/// <summary>One tracked company as declared by the universe source.</summary>
public sealed record UniverseEntry(
    string Symbol,
    string Name,
    string Sector,
    string? Industry,
    string? Exchange);

/// <summary>The full set of companies the application should collect prices for.</summary>
public sealed record StockUniverse(string Version, IReadOnlyList<UniverseEntry> Symbols);

/// <summary>
/// Supplies the tracked universe. Backed by a JSON file today; a database-backed or remote
/// implementation can replace it without touching the sync logic.
/// </summary>
public interface IStockUniverseSource
{
    string Description { get; }

    Task<StockUniverse> LoadAsync(CancellationToken cancellationToken = default);
}
