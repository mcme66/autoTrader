using System.Linq.Expressions;

using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Application.Abstractions.Persistence.Projections;
using FinanceAnalysis.Application.Abstractions.Persistence.Queries;
using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Domain.Catalog;

using Microsoft.EntityFrameworkCore;

namespace FinanceAnalysis.Infrastructure.Persistence.Repositories;

internal sealed class StockRepository(ApplicationDbContext db) : IStockRepository
{
    /// <summary>
    /// Projects a stock plus its two most recent closes. The correlated subqueries become
    /// LATERAL joins in Postgres and are index-only lookups against
    /// <c>ux_daily_prices_stock_trade_date</c>, so the cost stays flat as history grows.
    /// </summary>
    private static readonly Expression<Func<Stock, StockSummary>> ToSummary = stock => new StockSummary(
        stock.Id,
        stock.Symbol,
        stock.Company.Name,
        stock.Company.Sector != null ? stock.Company.Sector.Key : null,
        stock.Company.Sector != null ? stock.Company.Sector.Name : null,
        stock.Company.Industry != null ? stock.Company.Industry.Name : null,
        stock.Exchange,
        stock.CurrencyCode,
        stock.IsTracked,
        stock.DailyPrices
            .OrderByDescending(p => p.TradeDate)
            .Select(p => (DateOnly?)p.TradeDate)
            .FirstOrDefault(),
        stock.DailyPrices
            .OrderByDescending(p => p.TradeDate)
            .Select(p => (decimal?)p.Close)
            .FirstOrDefault(),
        stock.DailyPrices
            .OrderByDescending(p => p.TradeDate)
            .Skip(1)
            .Select(p => (decimal?)p.Close)
            .FirstOrDefault(),
        stock.DailyPrices
            .OrderByDescending(p => p.TradeDate)
            .Select(p => (long?)p.Volume)
            .FirstOrDefault());

    public Task<Stock?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Stocks.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<Stock?> FindBySymbolAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var normalized = Stock.NormalizeSymbol(symbol);
        return db.Stocks
            .Include(s => s.Company)
            .FirstOrDefaultAsync(s => s.Symbol == normalized, cancellationToken);
    }

    public Task<StockSummary?> GetSummaryBySymbolAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        var normalized = Stock.NormalizeSymbol(symbol);
        return db.Stocks
            .Where(s => s.Symbol == normalized)
            .Select(ToSummary)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<StockSummary>> SearchAsync(
        StockSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = db.Stocks.AsNoTracking().AsQueryable();

        if (criteria.TrackedOnly)
        {
            query = query.Where(s => s.IsTracked);
        }

        if (!string.IsNullOrWhiteSpace(criteria.SectorKey))
        {
            query = query.Where(s => s.Company.Sector != null && s.Company.Sector.Key == criteria.SectorKey);
        }

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            // Symbols match as a prefix, company names anywhere. At a few hundred rows a
            // sequential ILIKE scan is immaterial; if the universe grows past a few thousand,
            // a pg_trgm GIN index on company name is the drop-in upgrade.
            var term = criteria.SearchTerm.Trim();
            var prefix = $"{term}%";
            var contains = $"%{term}%";

            query = query.Where(s =>
                EF.Functions.ILike(s.Symbol, prefix) || EF.Functions.ILike(s.Company.Name, contains));
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        if (totalCount == 0)
        {
            return PagedResult.Empty<StockSummary>(criteria.Page.Page, criteria.Page.PageSize);
        }

        query = ApplyOrdering(query, criteria);

        var items = await query
            .Skip(criteria.Page.Skip)
            .Take(criteria.Page.PageSize)
            .Select(ToSummary)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<StockSummary>(items, criteria.Page.Page, criteria.Page.PageSize, totalCount);
    }

    public async Task<IReadOnlyDictionary<string, int>> GetTrackedSymbolIdsAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await db.Stocks
            .AsNoTracking()
            .Where(s => s.IsTracked)
            .Select(s => new { s.Symbol, s.Id })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.ToDictionary(r => r.Symbol, r => r.Id, StringComparer.Ordinal);
    }

    public async Task<IReadOnlyList<Stock>> GetAllWithCompanyAsync(CancellationToken cancellationToken = default) =>
        await db.Stocks
            .Include(s => s.Company)
            .OrderBy(s => s.Symbol)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public Task<int> CountTrackedAsync(CancellationToken cancellationToken = default) =>
        db.Stocks.CountAsync(s => s.IsTracked, cancellationToken);

    public void Add(Stock stock) => db.Stocks.Add(stock);

    private static IQueryable<Stock> ApplyOrdering(IQueryable<Stock> query, StockSearchCriteria criteria) =>
        (criteria.SortBy, criteria.Descending) switch
        {
            (StockSortOrder.CompanyName, false) => query.OrderBy(s => s.Company.Name).ThenBy(s => s.Symbol),
            (StockSortOrder.CompanyName, true) => query.OrderByDescending(s => s.Company.Name).ThenBy(s => s.Symbol),
            (StockSortOrder.Sector, false) => query
                .OrderBy(s => s.Company.Sector!.DisplayOrder)
                .ThenBy(s => s.Symbol),
            (StockSortOrder.Sector, true) => query
                .OrderByDescending(s => s.Company.Sector!.DisplayOrder)
                .ThenBy(s => s.Symbol),
            (_, true) => query.OrderByDescending(s => s.Symbol),
            _ => query.OrderBy(s => s.Symbol),
        };
}
