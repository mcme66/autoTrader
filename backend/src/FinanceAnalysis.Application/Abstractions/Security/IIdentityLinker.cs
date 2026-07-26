using FinanceAnalysis.Application.Features.Authentication;

namespace FinanceAnalysis.Application.Abstractions.Security;

/// <summary>
/// Turns an identity asserted by an external provider into a local session.
/// </summary>
/// <remarks>
/// No OAuth handler is wired up today. This seam and the <c>external_logins</c> table exist so
/// that adding one later is purely additive: register <c>.AddGoogle()</c>, call
/// <see cref="SignInWithExternalProviderAsync"/> from the callback, and nothing about the user
/// model, the token pipeline or the frontend changes.
/// </remarks>
public interface IIdentityLinker
{
    /// <summary>
    /// Signs in the user behind <paramref name="providerKey"/>, linking the external identity
    /// to an existing account with the same verified email or creating a new account.
    /// </summary>
    /// <param name="provider">Provider discriminator, for example "Google".</param>
    /// <param name="providerKey">The provider's stable subject identifier.</param>
    /// <param name="email">The verified email asserted by the provider.</param>
    /// <param name="displayName">Display name asserted by the provider.</param>
    /// <param name="ipAddress">Caller address, recorded against the refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<AuthenticationResult> SignInWithExternalProviderAsync(
        string provider,
        string providerKey,
        string email,
        string displayName,
        string? ipAddress,
        CancellationToken cancellationToken = default);
}
