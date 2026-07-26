using FinanceAnalysis.Application.Abstractions.Persistence.Queries;
using FinanceAnalysis.Application.Common;

namespace FinanceAnalysis.Application.Features.Stocks;

public interface IStockService
{
    Task<PagedResult<StockDto>> SearchAsync(
        StockSearchCriteria criteria,
        CancellationToken cancellationToken = default);

    Task<StockDetailDto> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Price history for a symbol. The window defaults to the last year when unspecified and is
    /// clamped so a single request cannot ask for an unbounded scan.
    /// </summary>
    Task<PriceHistoryDto> GetPriceHistoryAsync(
        string symbol,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts or stops collecting prices for a symbol. History is retained either way, which is
    /// what makes this safe to toggle at runtime.
    /// </summary>
    Task<StockDto> SetTrackingAsync(
        string symbol,
        bool isTracked,
        CancellationToken cancellationToken = default);
}
