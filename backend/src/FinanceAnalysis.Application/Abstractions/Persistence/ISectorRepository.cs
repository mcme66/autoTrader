using FinanceAnalysis.Domain.Catalog;

namespace FinanceAnalysis.Application.Abstractions.Persistence;

/// <summary>
/// Sectors and their child industries are read and written together during universe sync,
/// so they share a repository.
/// </summary>
public interface ISectorRepository
{
    Task<IReadOnlyList<Sector>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Sector?> FindByKeyAsync(string key, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Industry>> GetIndustriesAsync(CancellationToken cancellationToken = default);

    void AddIndustry(Industry industry);
}
