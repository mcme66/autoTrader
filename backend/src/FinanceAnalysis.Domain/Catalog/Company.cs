using FinanceAnalysis.Domain.Common;

namespace FinanceAnalysis.Domain.Catalog;

/// <summary>
/// The issuing business behind one or more listed <see cref="Stock"/> symbols. Kept
/// separate from <see cref="Stock"/> so that multiple share classes, and later fundamentals
/// such as earnings or filings, hang off a single company record.
/// </summary>
public sealed class Company : Entity<int>, IAuditable
{
    private readonly List<Stock> _stocks = [];

    private Company()
    {
    }

    public Company(string name)
    {
        Name = name;
    }

    public string Name { get; private set; } = null!;

    public int? SectorId { get; private set; }

    public int? IndustryId { get; private set; }

    /// <summary>SEC Central Index Key, when the provider supplies one.</summary>
    public string? Cik { get; private set; }

    public string? Description { get; private set; }

    public string? HomepageUrl { get; private set; }

    public string? CountryCode { get; private set; }

    public int? EmployeeCount { get; private set; }

    public DateOnly? ListedOn { get; private set; }

    /// <summary>Set when a provider profile was last merged into this record.</summary>
    public DateTimeOffset? ProfileRefreshedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Sector? Sector { get; private set; }

    public Industry? Industry { get; private set; }

    public IReadOnlyCollection<Stock> Stocks => _stocks;

    public void Rename(string name) => Name = name;

    public void Classify(int? sectorId, int? industryId)
    {
        SectorId = sectorId;
        IndustryId = industryId;
    }

    /// <summary>
    /// Merges a provider profile, leaving existing values in place where the provider
    /// returned nothing. Providers differ in coverage, so a sparse response must never
    /// erase data another provider already supplied.
    /// </summary>
    public void ApplyProfile(
        string? description,
        string? homepageUrl,
        string? countryCode,
        string? cik,
        int? employeeCount,
        DateOnly? listedOn,
        DateTimeOffset refreshedAt)
    {
        Description = description ?? Description;
        HomepageUrl = homepageUrl ?? HomepageUrl;
        CountryCode = countryCode ?? CountryCode;
        Cik = cik ?? Cik;
        EmployeeCount = employeeCount ?? EmployeeCount;
        ListedOn = listedOn ?? ListedOn;
        ProfileRefreshedAt = refreshedAt;
    }
}
