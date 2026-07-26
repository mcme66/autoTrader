using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Application.Abstractions.Persistence.Projections;
using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Domain.Catalog;
using FinanceAnalysis.Domain.Predictions;

using Microsoft.EntityFrameworkCore;

namespace FinanceAnalysis.Infrastructure.Persistence.Repositories;

/// <summary>
/// Read-only access to the ML tables. Every query is <c>AsNoTracking</c> because nothing in
/// this application ever writes them back.
/// </summary>
internal sealed class PredictionRepository(ApplicationDbContext db) : IPredictionRepository
{
    public Task<bool> HasAnyPredictionsAsync(CancellationToken cancellationToken = default) =>
        db.MlPredictions.AsNoTracking().AnyAsync(cancellationToken);

    public async Task<IReadOnlyList<MlModel>> GetModelsAsync(CancellationToken cancellationToken = default) =>
        await db.MlModels
            .AsNoTracking()
            .OrderByDescending(m => m.IsActive)
            .ThenBy(m => m.Key)
            .ThenByDescending(m => m.Version)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<PagedResult<PredictionSummary>> GetLatestAsync(
        PageRequest page,
        string? modelKey = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.MlPredictions.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(modelKey))
        {
            query = query.Where(p => p.Model.Key == modelKey);
        }

        // One row per symbol: the newest prediction, tie-broken by the furthest target date.
        var newestPerStock = query
            .GroupBy(p => p.StockId)
            .Select(g => g
                .OrderByDescending(p => p.PredictionDate)
                .ThenByDescending(p => p.TargetDate)
                .Select(p => p.Id)
                .First());

        var latest = db.MlPredictions.AsNoTracking().Where(p => newestPerStock.Contains(p.Id));

        var totalCount = await latest.CountAsync(cancellationToken).ConfigureAwait(false);
        if (totalCount == 0)
        {
            return PagedResult.Empty<PredictionSummary>(page.Page, page.PageSize);
        }

        var items = await latest
            .OrderByDescending(p => p.Signal)
            .ThenByDescending(p => p.Confidence)
            .ThenBy(p => p.Stock.Symbol)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(ToSummary())
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<PredictionSummary>(items, page.Page, page.PageSize, totalCount);
    }

    public async Task<IReadOnlyList<PredictionSummary>> GetForSymbolAsync(
        string symbol,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var normalized = Stock.NormalizeSymbol(symbol);

        return await db.MlPredictions
            .AsNoTracking()
            .Where(p => p.Stock.Symbol == normalized)
            .OrderByDescending(p => p.PredictionDate)
            .ThenByDescending(p => p.TargetDate)
            .Take(limit)
            .Select(ToSummary())
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<PredictionAccuracy>> GetModelAccuracyAsync(
        CancellationToken cancellationToken = default) =>
        await db.MlPredictionHistory
            .AsNoTracking()
            .Where(h => h.ActualValue != null)
            .GroupBy(h => new { h.Model.Key, h.Model.Name, h.Model.Version })
            .Select(g => new PredictionAccuracy(
                g.Key.Key,
                g.Key.Name,
                g.Key.Version,
                g.Count(),
                g.Average(h => h.AbsoluteError),
                g.Average(h => h.PercentageError),
                g.Count(h => h.DirectionCorrect == true) * 100m / g.Count()))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    private static System.Linq.Expressions.Expression<Func<MlPrediction, PredictionSummary>> ToSummary() =>
        p => new PredictionSummary(
            p.Id,
            p.Stock.Symbol,
            p.Stock.Company.Name,
            p.Stock.Company.Sector != null ? p.Stock.Company.Sector.Name : null,
            p.Model.Key,
            p.Model.Name,
            p.Model.Version,
            p.PredictionDate,
            p.TargetDate,
            p.HorizonDays,
            p.PredictedClose,
            p.PredictedReturn,
            p.Direction,
            p.Signal,
            p.Confidence,
            p.Stock.DailyPrices
                .OrderByDescending(d => d.TradeDate)
                .Select(d => (decimal?)d.Close)
                .FirstOrDefault());
}
