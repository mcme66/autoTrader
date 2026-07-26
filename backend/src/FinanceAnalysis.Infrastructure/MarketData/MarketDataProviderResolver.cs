using FinanceAnalysis.Application.Abstractions.MarketData;
using FinanceAnalysis.Application.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FinanceAnalysis.Infrastructure.MarketData;

/// <summary>
/// Resolves the active provider from keyed DI using the <c>MarketData:Provider</c> setting.
/// </summary>
/// <remarks>
/// Keyed services mean every provider can be registered simultaneously and selected at
/// runtime, so switching vendors is a configuration change and a provider can be exercised
/// directly in tests without reconfiguring the container.
/// </remarks>
internal sealed class MarketDataProviderResolver(
    IServiceProvider services,
    IOptions<MarketDataOptions> options,
    MarketDataProviderRegistry registry) : IMarketDataProviderResolver
{
    private readonly MarketDataOptions _options = options.Value;

    public IReadOnlyList<string> AvailableKeys { get; } = registry.Keys;

    public IMarketDataProvider Resolve() => Resolve(_options.Provider);

    public IMarketDataProvider Resolve(string key)
    {
        var provider = services.GetKeyedService<IMarketDataProvider>(key);

        if (provider is null)
        {
            throw new InvalidOperationException(
                $"No market data provider is registered under the key '{key}'. "
                + $"Registered providers: {string.Join(", ", AvailableKeys)}.");
        }

        return provider;
    }
}
