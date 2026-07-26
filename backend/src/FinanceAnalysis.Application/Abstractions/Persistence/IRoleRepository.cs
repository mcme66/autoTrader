using FinanceAnalysis.Domain.Identity;

namespace FinanceAnalysis.Application.Abstractions.Persistence;

public interface IRoleRepository
{
    Task<Role?> FindByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default);
}
