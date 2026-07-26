using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Domain.Identity;

using Microsoft.EntityFrameworkCore;

namespace FinanceAnalysis.Infrastructure.Persistence.Repositories;

internal sealed class RefreshTokenRepository(ApplicationDbContext db) : IRefreshTokenRepository
{
    public Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public async Task<IReadOnlyList<RefreshToken>> GetActiveForUserAsync(
        Guid userId,
        DateTimeOffset asOf,
        CancellationToken cancellationToken = default) =>
        await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > asOf)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public void Add(RefreshToken token) => db.RefreshTokens.Add(token);

    public Task<int> DeleteExpiredAsync(DateTimeOffset cutoff, CancellationToken cancellationToken = default) =>
        db.RefreshTokens
            .Where(t => t.ExpiresAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);
}
