namespace FinanceAnalysis.Application.Abstractions.MarketData;

/// <summary>
/// Descriptive company data from a provider. Every field beyond the symbol is optional
/// because provider coverage varies; the merge into <c>Company</c> leaves existing values
/// untouched where a field comes back null.
/// </summary>
public sealed record CompanyProfile(
    string Symbol,
    string? Name = null,
    string? Description = null,
    string? HomepageUrl = null,
    string? CountryCode = null,
    string? Cik = null,
    string? IndustryName = null,
    int? EmployeeCount = null,
    DateOnly? ListedOn = null);
