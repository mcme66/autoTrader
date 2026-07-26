using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using FinanceAnalysis.Application.Abstractions.Security;
using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Application.Configuration;
using FinanceAnalysis.Domain.Identity;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FinanceAnalysis.Infrastructure.Security;

/// <summary>
/// Issues signed access tokens and opaque refresh tokens.
/// </summary>
/// <remarks>
/// Access tokens are self-contained JWTs and therefore cannot be revoked, which is why they
/// are short-lived. Refresh tokens are opaque random values; only their SHA-256 digest is
/// stored, so a database compromise does not yield usable session credentials.
/// </remarks>
internal sealed class JwtTokenService : ITokenService
{
    private const int RefreshTokenBytes = 32;

    private readonly JwtOptions _options;
    private readonly IClock _clock;
    private readonly SigningCredentials _signingCredentials;

    public JwtTokenService(IOptions<AuthenticationOptions> options, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options.Value.Jwt;
        _clock = clock;

        if (string.IsNullOrWhiteSpace(_options.SigningKey))
        {
            throw new InvalidOperationException(
                "Auth:Jwt:SigningKey is not configured. Set the Auth__Jwt__SigningKey environment variable "
                + "or use 'dotnet user-secrets set Auth:Jwt:SigningKey <value>' for local development.");
        }

        var keyBytes = Encoding.UTF8.GetBytes(_options.SigningKey);

        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException(
                "Auth:Jwt:SigningKey must be at least 32 bytes so it can key HMAC-SHA256.");
        }

        _signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(keyBytes),
            SecurityAlgorithms.HmacSha256);
    }

    public AccessToken CreateAccessToken(User user, IReadOnlyCollection<string> roles)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(roles);

        var issuedAt = _clock.UtcNow;
        var expiresAt = issuedAt.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>(roles.Count + 5)
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.DisplayName),
            new(
                JwtRegisteredClaimNames.Iat,
                issuedAt.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64),
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: issuedAt.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: _signingCredentials);

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public RefreshTokenPair CreateRefreshToken()
    {
        var value = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(RefreshTokenBytes));

        return new RefreshTokenPair(
            value,
            HashRefreshToken(value),
            _clock.UtcNow.AddDays(_options.RefreshTokenDays));
    }

    public string HashRefreshToken(string refreshToken) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
}
