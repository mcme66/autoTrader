using FinanceAnalysis.Application.Abstractions.MarketData;
using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Application.Configuration;
using FinanceAnalysis.Domain.Catalog;
using FinanceAnalysis.Domain.Enums;
using FinanceAnalysis.Domain.Exceptions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceAnalysis.Application.Features.Universe;

/// <summary>
/// Reconciles the declared universe into the catalogue tables.
/// </summary>
/// <remarks>
/// This is a diff rather than a rebuild, and it never deletes. A symbol that disappears from
/// the file is marked untracked, which stops future ingestion while leaving every bar already
/// collected in place and queryable. Re-adding the symbol later resumes tracking against the
/// same history. That is what lets the tracked list change at runtime without a code change
/// and without losing data.
/// </remarks>
public sealed class UniverseSyncService(
    IStockUniverseSource universeSource,
    IStockRepository stocks,
    ICompanyRepository companies,
    ISectorRepository sectors,
    IUnitOfWork unitOfWork,
    IOptions<MarketDataOptions> options,
    ILogger<UniverseSyncService> logger) : IUniverseSyncService
{
    private const string DefaultCurrency = "USD";

    private readonly MarketDataOptions _options = options.Value;

    public async Task<UniverseSyncResult> SyncAsync(CancellationToken cancellationToken = default)
    {
        var universe = await universeSource.LoadAsync(cancellationToken).ConfigureAwait(false);

        if (universe.Symbols.Count == 0)
        {
            throw new BusinessRuleException("The universe source returned no symbols.");
        }

        if (universe.Symbols.Count > _options.MaxTrackedSymbols)
        {
            throw new BusinessRuleException(
                $"The universe declares {universe.Symbols.Count} symbols, which exceeds the configured "
                + $"maximum of {_options.MaxTrackedSymbols}.");
        }

        var warnings = new List<string>();
        var sectorsByKey = (await sectors.GetAllAsync(cancellationToken).ConfigureAwait(false))
            .ToDictionary(s => s.Key, StringComparer.OrdinalIgnoreCase);

        var industryCache = (await sectors.GetIndustriesAsync(cancellationToken).ConfigureAwait(false))
            .ToDictionary(i => IndustryKey(i.SectorId, i.Name), StringComparer.Ordinal);

        var existingStocks = (await stocks.GetAllWithCompanyAsync(cancellationToken).ConfigureAwait(false))
            .ToDictionary(s => s.Symbol, StringComparer.Ordinal);

        var declaredSymbols = new HashSet<string>(StringComparer.Ordinal);
        int added = 0, updated = 0, retracked = 0, industriesCreated = 0;

        foreach (var entry in universe.Symbols)
        {
            var symbol = Stock.NormalizeSymbol(entry.Symbol);

            if (!declaredSymbols.Add(symbol))
            {
                warnings.Add($"Symbol '{symbol}' appears more than once in the universe; the duplicate was ignored.");
                continue;
            }

            if (!sectorsByKey.TryGetValue(entry.Sector, out var sector))
            {
                warnings.Add(
                    $"Symbol '{symbol}' references unknown sector '{entry.Sector}' and was left unclassified.");
            }

            int? industryId = null;
            if (sector is not null && !string.IsNullOrWhiteSpace(entry.Industry))
            {
                var industryName = entry.Industry.Trim();
                var key = IndustryKey(sector.Id, industryName);

                if (!industryCache.TryGetValue(key, out var industry))
                {
                    industry = new Industry(sector.Id, industryName);
                    sectors.AddIndustry(industry);
                    industryCache[key] = industry;
                    industriesCreated++;

                    // The identity value is assigned on save; flush so later stocks in this
                    // same run can reference the industry by id.
                    await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                industryId = industry.Id;
            }

            if (existingStocks.TryGetValue(symbol, out var stock))
            {
                var wasTracked = stock.IsTracked;

                stock.Company.Rename(entry.Name);
                stock.Company.Classify(sector?.Id, industryId);
                stock.UpdateListing(entry.Exchange, AssetType.CommonStock);
                stock.StartTracking();

                if (wasTracked)
                {
                    updated++;
                }
                else
                {
                    retracked++;
                }
            }
            else
            {
                var company = new Company(entry.Name);
                company.Classify(sector?.Id, industryId);
                companies.Add(company);

                // Persist so the new company has an id for the stock's foreign key.
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                stocks.Add(new Stock(symbol, company.Id, entry.Exchange, DefaultCurrency, AssetType.CommonStock));
                added++;
            }
        }

        var untracked = 0;
        foreach (var (symbol, stock) in existingStocks)
        {
            if (stock.IsTracked && !declaredSymbols.Contains(symbol))
            {
                stock.StopTracking();
                untracked++;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var result = new UniverseSyncResult(
            universe.Version,
            declaredSymbols.Count,
            added,
            updated,
            retracked,
            untracked,
            industriesCreated,
            warnings);

        logger.LogInformation(
            "Universe sync from {Source} (version {Version}) completed: {Added} added, {Updated} updated, "
            + "{Retracked} re-tracked, {Untracked} untracked, {IndustriesCreated} industries created, "
            + "{WarningCount} warnings.",
            universeSource.Description,
            universe.Version,
            added,
            updated,
            retracked,
            untracked,
            industriesCreated,
            warnings.Count);

        foreach (var warning in warnings)
        {
            logger.LogWarning("Universe sync warning: {Warning}", warning);
        }

        return result;
    }

    /// <summary>
    /// Industry names are unique per sector and matched case-insensitively, so that a casing
    /// change in the universe file does not create a duplicate industry row.
    /// </summary>
    private static string IndustryKey(int sectorId, string name) =>
        $"{sectorId}|{name.ToUpperInvariant()}";
}
