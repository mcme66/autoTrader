using FinanceAnalysis.Domain.Identity;

namespace FinanceAnalysis.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Finds a user together with their role assignments, for token issuance.</summary>
    Task<User?> FindByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> FindByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(User user);
}
