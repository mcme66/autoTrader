namespace FinanceAnalysis.Application.Features.Authentication;

/// <summary>Registration payload.</summary>
public sealed record RegisterRequest(string Email, string Password, string DisplayName);

/// <summary>Sign-in payload.</summary>
public sealed record LoginRequest(string Email, string Password);

/// <summary>Password change payload for an already-authenticated user.</summary>
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

/// <summary>The authenticated user as exposed to clients.</summary>
public sealed record AuthenticatedUser(
    Guid Id,
    string Email,
    string DisplayName,
    IReadOnlyList<string> Roles,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastLoginAt);

/// <summary>
/// The outcome of a successful sign-in, registration or refresh.
/// </summary>
/// <remarks>
/// <see cref="RefreshToken"/> is the raw value and is written to an httpOnly cookie by the
/// controller rather than into the response body, so page scripts cannot read it.
/// </remarks>
public sealed record AuthenticationResult(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    AuthenticatedUser User);
