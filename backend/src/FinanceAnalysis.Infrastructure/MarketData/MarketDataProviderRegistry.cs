namespace FinanceAnalysis.Infrastructure.MarketData;

/// <summary>
/// The set of provider keys registered in the container.
/// </summary>
/// <remarks>
/// Keyed DI can resolve a service by key but cannot enumerate the keys, so the registration
/// code records them here. A dedicated type rather than a bare collection keeps the container
/// free of ambiguous <c>IEnumerable&lt;string&gt;</c> registrations.
/// </remarks>
internal sealed record MarketDataProviderRegistry(IReadOnlyList<string> Keys);
