using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Domain.Catalog;

using Microsoft.EntityFrameworkCore;

namespace FinanceAnalysis.Infrastructure.Persistence.Repositories;

internal sealed class SectorRepository(ApplicationDbContext db) : ISectorRepository
{
    public async Task<IReadOnlyList<Sector>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Sectors
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public Task<Sector?> FindByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        db.Sectors.FirstOrDefaultAsync(s => s.Key == key, cancellationToken);

    public async Task<IReadOnlyList<Industry>> GetIndustriesAsync(CancellationToken cancellationToken = default) =>
        await db.Industries.ToListAsync(cancellationToken).ConfigureAwait(false);

    public void AddIndustry(Industry industry) => db.Industries.Add(industry);
}
