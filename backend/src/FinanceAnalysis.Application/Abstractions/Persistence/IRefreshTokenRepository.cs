using FinanceAnalysis.Domain.Identity;

namespace FinanceAnalysis.Application.Abstractions.Persistence;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RefreshToken>> GetActiveForUserAsync(
        Guid userId,
        DateTimeOffset asOf,
        CancellationToken cancellationToken = default);

    void Add(RefreshToken token);

    /// <summary>Deletes tokens that expired before <paramref name="cutoff"/>.</summary>
    Task<int> DeleteExpiredAsync(DateTimeOffset cutoff, CancellationToken cancellationToken = default);
}
