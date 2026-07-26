using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Domain.Identity;

using Microsoft.EntityFrameworkCore;

namespace FinanceAnalysis.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(ApplicationDbContext db) : IUserRepository
{
    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = User.NormalizeEmail(email);
        return db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, cancellationToken);
    }

    public Task<User?> FindByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = User.NormalizeEmail(email);
        return db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, cancellationToken);
    }

    public Task<User?> FindByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = User.NormalizeEmail(email);
        return db.Users.AnyAsync(u => u.NormalizedEmail == normalized, cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        db.Users.CountAsync(cancellationToken);

    public async Task<IReadOnlyList<string>> GetRoleNamesAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public void Add(User user) => db.Users.Add(user);
}
