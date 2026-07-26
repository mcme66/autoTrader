using FinanceAnalysis.Application.Abstractions.Persistence.Projections;
using FinanceAnalysis.Application.Abstractions.Persistence.Queries;
using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Domain.Catalog;

namespace FinanceAnalysis.Application.Abstractions.Persistence;

public interface IStockRepository
{
    Task<Stock?> FindByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Stock?> FindBySymbolAsync(string symbol, CancellationToken cancellationToken = default);

    Task<StockSummary?> GetSummaryBySymbolAsync(string symbol, CancellationToken cancellationToken = default);

    Task<PagedResult<StockSummary>> SearchAsync(
        StockSearchCriteria criteria,
        CancellationToken cancellationToken = default);

    /// <summary>Symbols the daily ingestion job should collect, keyed by symbol.</summary>
    Task<IReadOnlyDictionary<string, int>> GetTrackedSymbolIdsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Stock>> GetAllWithCompanyAsync(CancellationToken cancellationToken = default);

    Task<int> CountTrackedAsync(CancellationToken cancellationToken = default);

    void Add(Stock stock);
}
