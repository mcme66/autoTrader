using System.ComponentModel.DataAnnotations;

namespace FinanceAnalysis.Application.Configuration;

/// <summary>
/// Authentication settings, bound from the <c>Auth</c> section.
/// </summary>
public sealed class AuthenticationOptions
{
    public const string SectionName = "Auth";

    /// <summary>
    /// When false, <c>POST /api/auth/register</c> returns 403. Intended to be switched off once
    /// the intended accounts exist, since this deployment is not a public signup product.
    /// </summary>
    public bool AllowRegistration { get; set; } = true;

    /// <summary>
    /// Grants the administrator role to the first account created. This is how the internal
    /// endpoints get an owner without shipping a default password.
    /// </summary>
    public bool FirstUserIsAdmin { get; set; } = true;

    [Range(6, 128)]
    public int MinimumPasswordLength { get; set; } = 12;

    /// <summary>
    /// BCrypt cost factor. Each increment doubles hashing time; 12 is a reasonable 2020s
    /// default and is deliberately configurable so it can be raised as hardware improves.
    /// </summary>
    [Range(10, 16)]
    public int PasswordHashWorkFactor { get; set; } = 12;

    public JwtOptions Jwt { get; set; } = new();
}

/// <summary>Token issuance settings.</summary>
public sealed class JwtOptions
{
    [Required]
    public string Issuer { get; set; } = "FinanceAnalysisPlatform";

    [Required]
    public string Audience { get; set; } = "FinanceAnalysisPlatform";

    /// <summary>
    /// HMAC signing key, at least 32 bytes. Supplied through <c>Auth__Jwt__SigningKey</c> or
    /// user-secrets; the application refuses to start in production without it.
    /// </summary>
    public string? SigningKey { get; set; }

    /// <summary>
    /// Access tokens are deliberately short-lived: they cannot be revoked, so the refresh
    /// token (which can) is what actually bounds a session.
    /// </summary>
    [Range(1, 1440)]
    public int AccessTokenMinutes { get; set; } = 15;

    [Range(1, 365)]
    public int RefreshTokenDays { get; set; } = 14;
}
