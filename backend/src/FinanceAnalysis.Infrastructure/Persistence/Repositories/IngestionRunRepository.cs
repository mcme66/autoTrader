using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Domain.Enums;
using FinanceAnalysis.Domain.MarketData;

using Microsoft.EntityFrameworkCore;

namespace FinanceAnalysis.Infrastructure.Persistence.Repositories;

internal sealed class IngestionRunRepository(ApplicationDbContext db) : IIngestionRunRepository
{
    public Task<IngestionRun?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.IngestionRuns
            .Include(r => r.DataSource)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<PagedResult<IngestionRun>> SearchAsync(
        PageRequest page,
        IngestionRunType? runType,
        IngestionRunStatus? status,
        CancellationToken cancellationToken = default)
    {
        IQueryable<IngestionRun> query = db.IngestionRuns.AsNoTracking().Include(r => r.DataSource);

        if (runType is not null)
        {
            query = query.Where(r => r.RunType == runType);
        }

        if (status is not null)
        {
            query = query.Where(r => r.Status == status);
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await query
            .OrderByDescending(r => r.QueuedAt)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<IngestionRun>(items, page.Page, page.PageSize, totalCount);
    }

    public Task<bool> HasSucceededForDateAsync(
        IngestionRunType runType,
        DateOnly tradeDate,
        CancellationToken cancellationToken = default) =>
        db.IngestionRuns.AnyAsync(
            r => r.RunType == runType
                && r.TradeDate == tradeDate
                && (r.Status == IngestionRunStatus.Succeeded || r.Status == IngestionRunStatus.PartiallySucceeded),
            cancellationToken);

    public void Add(IngestionRun run) => db.IngestionRuns.Add(run);
}
