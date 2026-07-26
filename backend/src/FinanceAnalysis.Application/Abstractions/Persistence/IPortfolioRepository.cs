using FinanceAnalysis.Application.Abstractions.Persistence.Projections;
using FinanceAnalysis.Domain.Portfolios;

namespace FinanceAnalysis.Application.Abstractions.Persistence;

public interface IPortfolioRepository
{
    Task<IReadOnlyList<Portfolio>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Portfolio?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Loads a portfolio with its holdings attached, for write operations.</summary>
    Task<Portfolio?> FindByIdWithHoldingsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Portfolio?> FindDefaultForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> NameExistsForUserAsync(
        Guid userId,
        string name,
        Guid? excludingPortfolioId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Holdings joined to their latest close, for valuing a portfolio in one query.</summary>
    Task<IReadOnlyList<HoldingValuation>> GetHoldingValuationsAsync(
        Guid portfolioId,
        CancellationToken cancellationToken = default);

    Task<int> CountHoldingsAsync(Guid portfolioId, CancellationToken cancellationToken = default);

    void Add(Portfolio portfolio);

    void Remove(Portfolio portfolio);
}
