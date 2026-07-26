using FinanceAnalysis.Domain.Identity;

namespace FinanceAnalysis.Application.Abstractions.Security;

/// <summary>A freshly issued access token and the moment it stops being accepted.</summary>
public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);

/// <summary>
/// A refresh token in both forms: the opaque value handed to the client, and the digest that
/// is all the database ever sees.
/// </summary>
public sealed record RefreshTokenPair(string Value, string Hash, DateTimeOffset ExpiresAt);

public interface ITokenService
{
    AccessToken CreateAccessToken(User user, IReadOnlyCollection<string> roles);

    RefreshTokenPair CreateRefreshToken();

    /// <summary>
    /// Hashes a refresh token presented by a client so it can be matched against stored
    /// digests. Must agree with the hashing done by <see cref="CreateRefreshToken"/>.
    /// </summary>
    string HashRefreshToken(string refreshToken);
}
