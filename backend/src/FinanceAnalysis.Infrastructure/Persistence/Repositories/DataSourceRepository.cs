using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Domain.MarketData;

using Microsoft.EntityFrameworkCore;

namespace FinanceAnalysis.Infrastructure.Persistence.Repositories;

internal sealed class DataSourceRepository(ApplicationDbContext db) : IDataSourceRepository
{
    public Task<DataSource?> FindByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        db.DataSources.FirstOrDefaultAsync(s => s.Key == key, cancellationToken);

    public async Task<DataSource> GetOrCreateAsync(
        string key,
        string name,
        CancellationToken cancellationToken = default)
    {
        var existing = await FindByKeyAsync(key, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            return existing;
        }

        var created = new DataSource(key, name);
        db.DataSources.Add(created);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return created;
    }
}
