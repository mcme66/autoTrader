using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Domain.Identity;

using Microsoft.EntityFrameworkCore;

namespace FinanceAnalysis.Infrastructure.Persistence.Repositories;

internal sealed class RoleRepository(ApplicationDbContext db) : IRoleRepository
{
    public Task<Role?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.ToUpperInvariant();
        return db.Roles.FirstOrDefaultAsync(r => r.NormalizedName == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Roles.OrderBy(r => r.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
}
