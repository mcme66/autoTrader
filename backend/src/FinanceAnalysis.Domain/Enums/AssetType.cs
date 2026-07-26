namespace FinanceAnalysis.Domain.Enums;

/// <summary>
/// The kind of instrument a <see cref="Catalog.Stock"/> row represents. Only
/// <see cref="CommonStock"/> is tracked today; the rest exist so that adding ETFs or
/// ADRs later does not require a schema change.
/// </summary>
public enum AssetType
{
    CommonStock = 0,
    PreferredStock = 1,
    Etf = 2,
    Adr = 3,
    Reit = 4,
}
