using FinanceAnalysis.Domain.Common;

namespace FinanceAnalysis.Domain.Catalog;

/// <summary>
/// A top-level market sector. Seeded with the eleven GICS sectors; the universe file
/// references sectors by <see cref="Key"/> so it never has to know database ids.
/// </summary>
public sealed class Sector : Entity<int>
{
    private readonly List<Industry> _industries = [];
    private readonly List<Company> _companies = [];

    private Sector()
    {
    }

    public Sector(string key, string name, int displayOrder)
    {
        Key = key;
        Name = name;
        DisplayOrder = displayOrder;
    }

    /// <summary>Stable machine-readable identifier, for example "InformationTechnology".</summary>
    public string Key { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public int DisplayOrder { get; private set; }

    public IReadOnlyCollection<Industry> Industries => _industries;

    public IReadOnlyCollection<Company> Companies => _companies;
}
