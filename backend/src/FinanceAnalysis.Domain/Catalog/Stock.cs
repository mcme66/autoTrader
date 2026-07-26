using FinanceAnalysis.Domain.Common;
using FinanceAnalysis.Domain.Enums;
using FinanceAnalysis.Domain.MarketData;

namespace FinanceAnalysis.Domain.Catalog;

/// <summary>
/// A tradable symbol. Rows are never deleted: dropping a company from the tracked universe
/// clears <see cref="IsTracked"/> so that its accumulated price history stays queryable.
/// </summary>
public sealed class Stock : Entity<int>, IAuditable
{
    private readonly List<DailyPrice> _dailyPrices = [];

    private Stock()
    {
    }

    public Stock(string symbol, int companyId, string? exchange, string currencyCode, AssetType assetType)
    {
        Symbol = NormalizeSymbol(symbol);
        CompanyId = companyId;
        Exchange = exchange;
        CurrencyCode = currencyCode;
        AssetType = assetType;
        IsTracked = true;
        TrackedSince = DateTimeOffset.UtcNow;
    }

    public string Symbol { get; private set; } = null!;

    public int CompanyId { get; private set; }

    /// <summary>Market Identifier Code of the primary listing venue, for example "XNAS".</summary>
    public string? Exchange { get; private set; }

    public string CurrencyCode { get; private set; } = null!;

    public AssetType AssetType { get; private set; }

    /// <summary>Whether the daily ingestion job should collect prices for this symbol.</summary>
    public bool IsTracked { get; private set; }

    public DateTimeOffset? TrackedSince { get; private set; }

    public DateTimeOffset? UntrackedAt { get; private set; }

    public DateOnly? DelistedOn { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Company Company { get; private set; } = null!;

    public IReadOnlyCollection<DailyPrice> DailyPrices => _dailyPrices;

    public static string NormalizeSymbol(string symbol) => symbol.Trim().ToUpperInvariant();

    public void StartTracking()
    {
        if (IsTracked)
        {
            return;
        }

        IsTracked = true;
        TrackedSince = DateTimeOffset.UtcNow;
        UntrackedAt = null;
    }

    public void StopTracking()
    {
        if (!IsTracked)
        {
            return;
        }

        IsTracked = false;
        UntrackedAt = DateTimeOffset.UtcNow;
    }

    public void MarkDelisted(DateOnly on)
    {
        DelistedOn = on;
        StopTracking();
    }

    public void UpdateListing(string? exchange, AssetType assetType)
    {
        Exchange = exchange ?? Exchange;
        AssetType = assetType;
    }
}
