namespace FinanceAnalysis.Application.Features.Portfolios;

/// <summary>
/// Portfolio and holding operations.
/// </summary>
/// <remarks>
/// Every method takes the caller's user id and enforces ownership itself rather than trusting
/// the route, so a valid token for one account can never reach another account's portfolio.
/// </remarks>
public interface IPortfolioService
{
    Task<IReadOnlyList<PortfolioDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<PortfolioDto> GetAsync(Guid userId, Guid portfolioId, CancellationToken cancellationToken = default);

    Task<PortfolioSummaryDto> GetSummaryAsync(
        Guid userId,
        Guid portfolioId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// The user's default portfolio valued in full, or null when they have none yet. Backs the
    /// dashboard, which must render for a brand new account.
    /// </summary>
    Task<PortfolioSummaryDto?> GetDefaultSummaryAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<PortfolioDto> CreateAsync(
        Guid userId,
        CreatePortfolioRequest request,
        CancellationToken cancellationToken = default);

    Task<PortfolioDto> UpdateAsync(
        Guid userId,
        Guid portfolioId,
        UpdatePortfolioRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid userId, Guid portfolioId, CancellationToken cancellationToken = default);

    Task<HoldingDto> AddHoldingAsync(
        Guid userId,
        Guid portfolioId,
        CreateHoldingRequest request,
        CancellationToken cancellationToken = default);

    Task<HoldingDto> UpdateHoldingAsync(
        Guid userId,
        Guid portfolioId,
        Guid holdingId,
        UpdateHoldingRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveHoldingAsync(
        Guid userId,
        Guid portfolioId,
        Guid holdingId,
        CancellationToken cancellationToken = default);
}
