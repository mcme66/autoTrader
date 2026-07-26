using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

using FinanceAnalysis.Api.Configuration;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace FinanceAnalysis.Api.Security;

/// <summary>Constants for the internal API key scheme.</summary>
public static class InternalApiKeyDefaults
{
    public const string AuthenticationScheme = "InternalApiKey";

    public const string HeaderName = "X-Internal-Api-Key";

    /// <summary>Role granted to a caller presenting a valid key.</summary>
    public const string Role = "InternalService";
}

/// <summary>
/// Authenticates the cron job that drives ingestion.
/// </summary>
/// <remarks>
/// A shared key rather than a user account, because the caller is a scheduled task with no
/// interactive session and no refresh-token lifecycle. This is one of four layers protecting
/// <c>/api/internal/*</c>: OpenAPI exclusion, this key, an IP allow-list, and an nginx deny
/// rule. Comparison is fixed-time so the key cannot be recovered by timing the endpoint.
/// </remarks>
internal sealed class InternalApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<SecurityOptions> securityOptions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly SecurityOptions _security = securityOptions.Value;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(InternalApiKeyDefaults.HeaderName, out var provided))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (string.IsNullOrWhiteSpace(_security.InternalApiKey))
        {
            Logger.LogError(
                "An internal endpoint was called but Security:InternalApiKey is not configured; "
                + "the request was rejected.");

            return Task.FromResult(AuthenticateResult.Fail("Internal access is not configured."));
        }

        if (!FixedTimeEquals(provided.ToString(), _security.InternalApiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("The internal API key is invalid."));
        }

        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, InternalApiKeyDefaults.Role),
                new Claim(ClaimTypes.Role, InternalApiKeyDefaults.Role),
            ],
            InternalApiKeyDefaults.AuthenticationScheme);

        var ticket = new AuthenticationTicket(
            new ClaimsPrincipal(identity),
            InternalApiKeyDefaults.AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private static bool FixedTimeEquals(string provided, string expected) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(provided),
            Encoding.UTF8.GetBytes(expected));
}
