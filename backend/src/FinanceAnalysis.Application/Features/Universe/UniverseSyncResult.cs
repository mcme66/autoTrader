namespace FinanceAnalysis.Application.Features.Universe;

/// <summary>
/// What a universe reconciliation changed. Returned to the caller and logged, so an operator
/// can confirm an edit to the universe file did what they expected.
/// </summary>
public sealed record UniverseSyncResult(
    string Version,
    int SymbolsInFile,
    int Added,
    int Updated,
    int Retracked,
    int Untracked,
    int IndustriesCreated,
    IReadOnlyList<string> Warnings)
{
    public int TotalTracked => SymbolsInFile;
}
