using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Application.Abstractions.Persistence.Projections;

using Microsoft.EntityFrameworkCore;

namespace FinanceAnalysis.Infrastructure.Persistence.Repositories;

/// <summary>
/// Reporting queries for the market overview screen.
/// </summary>
/// <remarks>
/// The five methods on this interface describe one screen built from one snapshot, so the
/// snapshot is fetched once per scope and the aggregates are derived from it in memory. That
/// is deliberate: the tracked universe is capped in the low hundreds, so the whole snapshot is
/// a single indexed query returning a few hundred narrow rows, and computing breadth and
/// movers in C# is both faster than five round trips and far easier to keep correct than five
/// hand-tuned aggregate queries. If the universe ever grows by orders of magnitude, the
/// replacement is a materialised view refreshed by the ingestion job, not more LINQ.
///
/// Everything is anchored to the newest trade date present in <c>daily_prices</c> rather than
/// to "today", so the screen stays meaningful over weekends, holidays, and any day the
/// ingestion job did not run.
/// </remarks>
internal sealed class MarketOverviewRepository(ApplicationDbContext db) : IMarketOverviewRepository
{
    private Snapshot? _snapshot;

    public async Task<MarketBreadth> GetBreadthAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await GetSnapshotAsync(cancellationToken).ConfigureAwait(false);

        if (snapshot.TradeDate is null)
        {
            return new MarketBreadth(null, 0, 0, 0, 0);
        }

        return new MarketBreadth(
            snapshot.TradeDate,
            snapshot.Rows.Count(r => r.ChangePercent > 0),
            snapshot.Rows.Count(r => r.ChangePercent < 0),
            snapshot.Rows.Count(r => r.ChangePercent is null or 0m),
            snapshot.Rows.Sum(r => r.LatestVolume ?? 0));
    }

    public async Task<IReadOnlyList<SectorPerformance>> GetSectorPerformanceAsync(
        CancellationToken cancellationToken = default)
    {
        var snapshot = await GetSnapshotAsync(cancellationToken).ConfigureAwait(false);

        var sectors = await db.Sectors
            .AsNoTracking()
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new { s.Key, s.Name })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var bySector = snapshot.Rows
            .Where(r => r.SectorKey is not null)
            .GroupBy(r => r.SectorKey!, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

        return
        [
            .. sectors.Select(sector =>
            {
                if (!bySector.TryGetValue(sector.Key, out var rows))
                {
                    return new SectorPerformance(sector.Key, sector.Name, 0, null, 0);
                }

                // Symbols with no prior close have no percentage change and are excluded from
                // the average rather than counted as flat, which would drag the sector toward
                // zero on the first day of collection.
                var changes = rows.Where(r => r.ChangePercent is not null).ToList();

                return new SectorPerformance(
                    sector.Key,
                    sector.Name,
                    rows.Count,
                    changes.Count == 0
                        ? null
                        : Math.Round(changes.Average(r => r.ChangePercent!.Value), 4, MidpointRounding.AwayFromZero),
                    rows.Sum(r => r.LatestVolume ?? 0));
            }),
        ];
    }

    public async Task<IReadOnlyList<StockSummary>> GetTopGainersAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await GetSnapshotAsync(cancellationToken).ConfigureAwait(false);

        return
        [
            .. snapshot.Rows
                .Where(r => r.ChangePercent is not null)
                .OrderByDescending(r => r.ChangePercent)
                .Take(count),
        ];
    }

    public async Task<IReadOnlyList<StockSummary>> GetTopLosersAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await GetSnapshotAsync(cancellationToken).ConfigureAwait(false);

        return
        [
            .. snapshot.Rows
                .Where(r => r.ChangePercent is not null)
                .OrderBy(r => r.ChangePercent)
                .Take(count),
        ];
    }

    public async Task<IReadOnlyList<StockSummary>> GetMostActiveAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await GetSnapshotAsync(cancellationToken).ConfigureAwait(false);

        return [.. snapshot.Rows.OrderByDescending(r => r.LatestVolume ?? 0).Take(count)];
    }

    private async Task<Snapshot> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        if (_snapshot is not null)
        {
            return _snapshot;
        }

        var tradeDate = await db.DailyPrices
            .AsNoTracking()
            .OrderByDescending(p => p.TradeDate)
            .Select(p => (DateOnly?)p.TradeDate)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (tradeDate is null)
        {
            return _snapshot = new Snapshot(null, []);
        }

        var rows = await db.Stocks
            .AsNoTracking()
            .Where(s => s.IsTracked && s.DailyPrices.Any(p => p.TradeDate == tradeDate))
            .Select(s => new StockSummary(
                s.Id,
                s.Symbol,
                s.Company.Name,
                s.Company.Sector != null ? s.Company.Sector.Key : null,
                s.Company.Sector != null ? s.Company.Sector.Name : null,
                s.Company.Industry != null ? s.Company.Industry.Name : null,
                s.Exchange,
                s.CurrencyCode,
                true,
                tradeDate,
                s.DailyPrices
                    .Where(p => p.TradeDate == tradeDate)
                    .Select(p => (decimal?)p.Close)
                    .FirstOrDefault(),
                s.DailyPrices
                    .Where(p => p.TradeDate < tradeDate)
                    .OrderByDescending(p => p.TradeDate)
                    .Select(p => (decimal?)p.Close)
                    .FirstOrDefault(),
                s.DailyPrices
                    .Where(p => p.TradeDate == tradeDate)
                    .Select(p => (long?)p.Volume)
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return _snapshot = new Snapshot(tradeDate, rows);
    }

    private sealed record Snapshot(DateOnly? TradeDate, IReadOnlyList<StockSummary> Rows);
}
