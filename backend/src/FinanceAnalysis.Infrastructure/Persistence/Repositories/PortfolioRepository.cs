using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Application.Abstractions.Persistence.Projections;
using FinanceAnalysis.Domain.Portfolios;

using Microsoft.EntityFrameworkCore;

namespace FinanceAnalysis.Infrastructure.Persistence.Repositories;

internal sealed class PortfolioRepository(ApplicationDbContext db) : IPortfolioRepository
{
    public async Task<IReadOnlyList<Portfolio>> GetForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await db.Portfolios
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.IsDefault)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public Task<Portfolio?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Portfolios.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Portfolio?> FindByIdWithHoldingsAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Portfolios
            .Include(p => p.Holdings)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Portfolio?> FindDefaultForUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        db.Portfolios.FirstOrDefaultAsync(p => p.UserId == userId && p.IsDefault, cancellationToken);

    public Task<bool> NameExistsForUserAsync(
        Guid userId,
        string name,
        Guid? excludingPortfolioId = null,
        CancellationToken cancellationToken = default) =>
        db.Portfolios.AnyAsync(
            p => p.UserId == userId
                && p.Name == name
                && (excludingPortfolioId == null || p.Id != excludingPortfolioId),
            cancellationToken);

    /// <summary>
    /// Values every holding in one round trip. Doing this per holding would issue N queries
    /// on a page that always renders all of them.
    /// </summary>
    public async Task<IReadOnlyList<HoldingValuation>> GetHoldingValuationsAsync(
        Guid portfolioId,
        CancellationToken cancellationToken = default) =>
        await db.PortfolioHoldings
            .AsNoTracking()
            .Where(h => h.PortfolioId == portfolioId)
            .OrderBy(h => h.Stock.Symbol)
            .Select(h => new HoldingValuation(
                h.Id,
                h.StockId,
                h.Stock.Symbol,
                h.Stock.Company.Name,
                h.Stock.Company.Sector != null ? h.Stock.Company.Sector.Key : null,
                h.Stock.Company.Sector != null ? h.Stock.Company.Sector.Name : null,
                h.Quantity,
                h.AverageCost,
                h.OpenedOn,
                h.Notes,
                h.Stock.DailyPrices
                    .OrderByDescending(p => p.TradeDate)
                    .Select(p => (decimal?)p.Close)
                    .FirstOrDefault(),
                h.Stock.DailyPrices
                    .OrderByDescending(p => p.TradeDate)
                    .Skip(1)
                    .Select(p => (decimal?)p.Close)
                    .FirstOrDefault(),
                h.Stock.DailyPrices
                    .OrderByDescending(p => p.TradeDate)
                    .Select(p => (DateOnly?)p.TradeDate)
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public Task<int> CountHoldingsAsync(Guid portfolioId, CancellationToken cancellationToken = default) =>
        db.PortfolioHoldings.CountAsync(h => h.PortfolioId == portfolioId, cancellationToken);

    public void Add(Portfolio portfolio) => db.Portfolios.Add(portfolio);

    public void Remove(Portfolio portfolio) => db.Portfolios.Remove(portfolio);
}
