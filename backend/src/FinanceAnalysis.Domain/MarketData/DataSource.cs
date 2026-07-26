using FinanceAnalysis.Domain.Common;

namespace FinanceAnalysis.Domain.MarketData;

/// <summary>
/// Records where a piece of market data came from. Every <see cref="DailyPrice"/> carries a
/// source, so data from a newly added provider is distinguishable from historical rows.
/// </summary>
public sealed class DataSource : Entity<int>
{
    private DataSource()
    {
    }

    public DataSource(string key, string name)
    {
        Key = key;
        Name = name;
    }

    /// <summary>Matches <c>IMarketDataProvider.Key</c>, for example "polygon".</summary>
    public string Key { get; private set; } = null!;

    public string Name { get; private set; } = null!;
}
