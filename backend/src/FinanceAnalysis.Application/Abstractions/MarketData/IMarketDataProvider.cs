namespace FinanceAnalysis.Application.Abstractions.MarketData;

/// <summary>
/// A source of market data.
/// </summary>
/// <remarks>
/// Adding a provider means writing one implementation of this interface and registering it
/// with <c>AddMarketDataProvider</c>; nothing else in the application changes. Implementations
/// own their own rate limiting and retry behaviour, because those differ per vendor plan.
/// </remarks>
public interface IMarketDataProvider
{
    /// <summary>
    /// Stable identifier matching the <c>MarketData:Provider</c> setting and the
    /// <c>data_sources.key</c> column, for example "polygon".
    /// </summary>
    string Key { get; }

    string DisplayName { get; }

    /// <summary>
    /// Fetches bars for <paramref name="tradeDate"/>, restricted to <paramref name="symbols"/>.
    /// Implementations should prefer whole-market endpoints where the vendor offers one, since
    /// that costs a single request instead of one per symbol.
    /// </summary>
    Task<DailyBarBatch> GetDailyBarsAsync(
        DateOnly tradeDate,
        IReadOnlySet<string> symbols,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches descriptive data for one symbol, or null when the provider has no profile
    /// endpoint or does not recognise the symbol.
    /// </summary>
    Task<CompanyProfile?> GetCompanyProfileAsync(string symbol, CancellationToken cancellationToken = default);
}
