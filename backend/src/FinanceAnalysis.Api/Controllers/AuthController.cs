using FinanceAnalysis.Api.Configuration;
using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Application.Features.Authentication;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FinanceAnalysis.Api.Controllers;

/// <summary>Registration, sign-in, token refresh and sign-out.</summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController(
    IAuthenticationService authentication,
    ICurrentUser currentUser,
    IOptions<SecurityOptions> securityOptions) : ControllerBase
{
    /// <summary>
    /// Name of the cookie carrying the refresh token.
    /// </summary>
    /// <remarks>
    /// The refresh token is never returned in the response body. Keeping it in an httpOnly
    /// cookie means an XSS flaw cannot exfiltrate a long-lived credential, and the SPA never
    /// has to store one.
    /// </remarks>
    private const string RefreshTokenCookie = "fap_refresh";

    private readonly SecurityOptions _security = securityOptions.Value;

    /// <summary>Creates an account and signs it in.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthenticationResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authentication.RegisterAsync(request, RemoteIp(), cancellationToken);
        return Respond(result);
    }

    /// <summary>Signs in with an email address and password.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authentication.LoginAsync(request, RemoteIp(), cancellationToken);
        return Respond(result);
    }

    /// <summary>Exchanges the refresh cookie for a new access token.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponse>> RefreshAsync(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(RefreshTokenCookie, out var refreshToken)
            || string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = "No refresh token was presented.",
                Status = StatusCodes.Status401Unauthorized,
            });
        }

        var result = await authentication.RefreshAsync(refreshToken, RemoteIp(), cancellationToken);
        return Respond(result);
    }

    /// <summary>Revokes the current refresh token. Always succeeds.</summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken)
    {
        Request.Cookies.TryGetValue(RefreshTokenCookie, out var refreshToken);
        await authentication.LogoutAsync(refreshToken, cancellationToken);

        Response.Cookies.Delete(RefreshTokenCookie, CookieOptions(DateTimeOffset.UnixEpoch));

        return NoContent();
    }

    /// <summary>Changes the signed-in user's password.</summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        await authentication.ChangePasswordAsync(currentUser.RequireUserId(), request, cancellationToken);
        return NoContent();
    }

    private ActionResult<AuthenticationResponse> Respond(AuthenticationResult result)
    {
        Response.Cookies.Append(
            RefreshTokenCookie,
            result.RefreshToken,
            CookieOptions(result.RefreshTokenExpiresAt));

        return Ok(new AuthenticationResponse(result.AccessToken, result.AccessTokenExpiresAt, result.User));
    }

    private CookieOptions CookieOptions(DateTimeOffset expiresAt) => new()
    {
        HttpOnly = true,
        Secure = _security.RequireSecureCookies,
        SameSite = SameSiteMode.Strict,
        Expires = expiresAt,

        // Scoped to the refresh endpoint so the cookie is not attached to every API call.
        Path = "/api/auth",
    };

    private string? RemoteIp() => HttpContext.Connection.RemoteIpAddress?.ToString();
}

/// <summary>
/// The body returned by the authentication endpoints. Excludes the refresh token by design;
/// it travels in an httpOnly cookie instead.
/// </summary>
public sealed record AuthenticationResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    AuthenticatedUser User);
