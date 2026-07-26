using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Application.Abstractions.Security;
using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Application.Configuration;
using FinanceAnalysis.Domain.Exceptions;
using FinanceAnalysis.Domain.Identity;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceAnalysis.Application.Features.Authentication;

/// <summary>
/// Registration, sign-in, refresh-token rotation and password changes.
/// </summary>
public sealed class AuthenticationService(
    IUserRepository users,
    IRoleRepository roles,
    IRefreshTokenRepository refreshTokens,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IUnitOfWork unitOfWork,
    IClock clock,
    IOptions<AuthenticationOptions> options,
    ILogger<AuthenticationService> logger) : IAuthenticationService, IIdentityLinker
{
    private const string InvalidCredentialsMessage = "The email address or password is incorrect.";

    private readonly AuthenticationOptions _options = options.Value;

    public async Task<AuthenticationResult> RegisterAsync(
        RegisterRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_options.AllowRegistration)
        {
            throw new ForbiddenException("Registration is closed on this deployment.");
        }

        if (await users.EmailExistsAsync(request.Email, cancellationToken).ConfigureAwait(false))
        {
            throw new ConflictException("An account with that email address already exists.");
        }

        var isFirstUser = await users.CountAsync(cancellationToken).ConfigureAwait(false) == 0;

        var user = User.CreateLocal(request.Email, request.DisplayName, passwordHasher.Hash(request.Password));

        var roleName = isFirstUser && _options.FirstUserIsAdmin ? RoleNames.Administrator : RoleNames.Member;
        var role = await roles.FindByNameAsync(roleName, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException(
                $"The '{roleName}' role is missing. Migrations seed it; the database may be out of date.");

        user.AssignRole(role);
        user.RecordLogin(clock.UtcNow);
        users.Add(user);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation(
            "Registered user {UserId} with role {Role}.",
            user.Id,
            roleName);

        return await IssueTokensAsync(user, [roleName], ipAddress, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AuthenticationResult> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await users
            .FindByEmailWithRolesAsync(request.Email, cancellationToken)
            .ConfigureAwait(false);

        // Verify against a dummy hash when the account does not exist so that the response
        // time does not reveal which emails are registered.
        if (user?.PasswordHash is null)
        {
            passwordHasher.Verify(request.Password, DummyHash);
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            logger.LogWarning("Failed sign-in attempt for user {UserId}.", user.Id);
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        if (!user.IsActive)
        {
            throw new ForbiddenException("This account has been deactivated.");
        }

        if (passwordHasher.NeedsRehash(user.PasswordHash))
        {
            user.ChangePassword(passwordHasher.Hash(request.Password));
            logger.LogInformation("Upgraded the password hash work factor for user {UserId}.", user.Id);
        }

        user.RecordLogin(clock.UtcNow);

        var roleNames = user.UserRoles.Select(ur => ur.Role.Name).ToArray();
        var result = await IssueTokensAsync(user, roleNames, ipAddress, cancellationToken).ConfigureAwait(false);

        logger.LogInformation("User {UserId} signed in.", user.Id);
        return result;
    }

    public async Task<AuthenticationResult> RefreshAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new UnauthorizedException("No refresh token was supplied.");
        }

        var now = clock.UtcNow;
        var hash = tokenService.HashRefreshToken(refreshToken);
        var stored = await refreshTokens.FindByHashAsync(hash, cancellationToken).ConfigureAwait(false)
            ?? throw new UnauthorizedException("The refresh token is not valid.");

        if (!stored.IsActive(now))
        {
            // A revoked token being presented again means either a stale client or a stolen
            // token being replayed. Revoking the whole family is the safe response.
            if (stored.IsRevoked)
            {
                await RevokeAllForUserAsync(stored.UserId, now, cancellationToken).ConfigureAwait(false);
                logger.LogWarning(
                    "A revoked refresh token was replayed for user {UserId}; all of their sessions were revoked.",
                    stored.UserId);
            }

            throw new UnauthorizedException("The refresh token has expired or been revoked.");
        }

        var user = await users.FindByIdWithRolesAsync(stored.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null || !user.IsActive)
        {
            stored.Revoke(now);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            throw new UnauthorizedException("The account is no longer active.");
        }

        var replacement = tokenService.CreateRefreshToken();
        stored.Revoke(now, replacement.Hash);
        refreshTokens.Add(new RefreshToken(user.Id, replacement.Hash, replacement.ExpiresAt, ipAddress));

        var accessToken = tokenService.CreateAccessToken(
            user,
            [.. user.UserRoles.Select(ur => ur.Role.Name)]);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AuthenticationResult(
            accessToken.Value,
            accessToken.ExpiresAt,
            replacement.Value,
            replacement.ExpiresAt,
            ToDto(user, [.. user.UserRoles.Select(ur => ur.Role.Name)]));
    }

    public async Task LogoutAsync(string? refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var hash = tokenService.HashRefreshToken(refreshToken);
        var stored = await refreshTokens.FindByHashAsync(hash, cancellationToken).ConfigureAwait(false);

        if (stored is null)
        {
            return;
        }

        stored.Revoke(clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("User {UserId} signed out.", stored.UserId);
    }

    public async Task ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await users.FindByIdAsync(userId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException("User", userId);

        if (user.PasswordHash is null)
        {
            throw new BusinessRuleException(
                "This account signs in through an external provider and has no password to change.");
        }

        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedException("The current password is incorrect.");
        }

        user.ChangePassword(passwordHasher.Hash(request.NewPassword));

        // Changing a password should end every other session.
        await RevokeAllForUserAsync(userId, clock.UtcNow, cancellationToken).ConfigureAwait(false);

        logger.LogInformation("User {UserId} changed their password; all sessions were revoked.", userId);
    }

    public async Task<AuthenticationResult> SignInWithExternalProviderAsync(
        string provider,
        string providerKey,
        string email,
        string displayName,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var user = await users.FindByEmailWithRolesAsync(email, cancellationToken).ConfigureAwait(false);

        if (user is null)
        {
            if (!_options.AllowRegistration)
            {
                throw new ForbiddenException("Registration is closed on this deployment.");
            }

            user = User.CreateExternal(email, displayName);

            var role = await roles.FindByNameAsync(RoleNames.Member, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("The 'Member' role is missing.");

            user.AssignRole(role);
            users.Add(user);
        }
        else if (!user.IsActive)
        {
            throw new ForbiddenException("This account has been deactivated.");
        }

        user.LinkExternalLogin(provider, providerKey);
        user.RecordLogin(clock.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var roleNames = user.UserRoles.Count > 0
            ? user.UserRoles.Select(ur => ur.Role.Name).ToArray()
            : await users.GetRoleNamesAsync(user.Id, cancellationToken).ConfigureAwait(false);

        return await IssueTokensAsync(user, roleNames, ipAddress, cancellationToken).ConfigureAwait(false);
    }

    private async Task<AuthenticationResult> IssueTokensAsync(
        User user,
        IReadOnlyList<string> roleNames,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var accessToken = tokenService.CreateAccessToken(user, roleNames);
        var refresh = tokenService.CreateRefreshToken();

        refreshTokens.Add(new RefreshToken(user.Id, refresh.Hash, refresh.ExpiresAt, ipAddress));
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AuthenticationResult(
            accessToken.Value,
            accessToken.ExpiresAt,
            refresh.Value,
            refresh.ExpiresAt,
            ToDto(user, roleNames));
    }

    private async Task RevokeAllForUserAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var active = await refreshTokens.GetActiveForUserAsync(userId, now, cancellationToken).ConfigureAwait(false);

        foreach (var token in active)
        {
            token.Revoke(now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static AuthenticatedUser ToDto(User user, IReadOnlyList<string> roleNames) => new(
        user.Id,
        user.Email,
        user.DisplayName,
        roleNames,
        user.CreatedAt,
        user.LastLoginAt);

    /// <summary>
    /// A real BCrypt hash of a value nobody knows, used purely to burn the same CPU time on a
    /// missing account as on a real one.
    /// </summary>
    private const string DummyHash = "$2a$12$K8Xk0Mc1YqAqLZ3rW9nJPeqTQx8h5v4eYd2sN1oU7bC6mA0gRfH3q";
}
