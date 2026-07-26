namespace FinanceAnalysis.Application.Features.Authentication;

public interface IAuthenticationService
{
    Task<AuthenticationResult> RegisterAsync(
        RegisterRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<AuthenticationResult> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exchanges a refresh token for a new token pair, revoking the one presented.
    /// </summary>
    Task<AuthenticationResult> RefreshAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>Revokes a refresh token. Idempotent, and never reveals whether it existed.</summary>
    Task LogoutAsync(string? refreshToken, CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default);
}
