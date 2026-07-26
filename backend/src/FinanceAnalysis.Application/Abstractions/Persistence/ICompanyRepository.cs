using FinanceAnalysis.Domain.Catalog;

namespace FinanceAnalysis.Application.Abstractions.Persistence;

public interface ICompanyRepository
{
    Task<Company?> FindByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the companies behind the given symbols. Used by universe sync, which needs to
    /// update existing companies rather than duplicate them.
    /// </summary>
    Task<IReadOnlyDictionary<string, Company>> GetBySymbolsAsync(
        IReadOnlyCollection<string> symbols,
        CancellationToken cancellationToken = default);

    void Add(Company company);
}
