namespace FinanceAnalysis.Application.Abstractions.MarketData;

/// <summary>
/// Selects the provider named by configuration. Resolution is indirected through this
/// interface so services depend on "the configured provider" rather than on a concrete
/// vendor, and so switching providers is a config change with no code change.
/// </summary>
public interface IMarketDataProviderResolver
{
    /// <summary>Returns the provider selected by <c>MarketData:Provider</c>.</summary>
    IMarketDataProvider Resolve();

    /// <summary>Returns a specific provider by key, or throws if it is not registered.</summary>
    IMarketDataProvider Resolve(string key);

    IReadOnlyList<string> AvailableKeys { get; }
}
