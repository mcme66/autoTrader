using FinanceAnalysis.Domain.MarketData;

namespace FinanceAnalysis.Application.Abstractions.Persistence;

public interface IDataSourceRepository
{
    Task<DataSource?> FindByKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the source for <paramref name="key"/>, creating it if a newly configured
    /// provider is being used for the first time.
    /// </summary>
    Task<DataSource> GetOrCreateAsync(string key, string name, CancellationToken cancellationToken = default);
}
