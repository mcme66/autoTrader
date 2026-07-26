using FinanceAnalysis.Domain.Common;

namespace FinanceAnalysis.Domain.Catalog;

/// <summary>
/// A sub-classification within a <see cref="Sector"/>. Industries are created on demand
/// from the universe file and from provider profile data, because not every provider
/// supplies one.
/// </summary>
public sealed class Industry : Entity<int>
{
    private readonly List<Company> _companies = [];

    private Industry()
    {
    }

    public Industry(int sectorId, string name)
    {
        SectorId = sectorId;
        Name = name;
    }

    public int SectorId { get; private set; }

    public string Name { get; private set; } = null!;

    public Sector Sector { get; private set; } = null!;

    public IReadOnlyCollection<Company> Companies => _companies;
}
