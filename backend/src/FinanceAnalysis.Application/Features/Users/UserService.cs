using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Application.Features.Authentication;
using FinanceAnalysis.Domain.Exceptions;

namespace FinanceAnalysis.Application.Features.Users;

/// <summary>Editable parts of a user's own profile.</summary>
public sealed record UpdateProfileRequest(string DisplayName);

public interface IUserService
{
    Task<AuthenticatedUser> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<AuthenticatedUser> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UserService(IUserRepository users, IUnitOfWork unitOfWork) : IUserService
{
    public async Task<AuthenticatedUser> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await users.FindByIdWithRolesAsync(userId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException("User", userId);

        return new AuthenticatedUser(
            user.Id,
            user.Email,
            user.DisplayName,
            [.. user.UserRoles.Select(ur => ur.Role.Name)],
            user.CreatedAt,
            user.LastLoginAt);
    }

    public async Task<AuthenticatedUser> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await users.FindByIdAsync(userId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException("User", userId);

        user.UpdateProfile(request.DisplayName);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await GetProfileAsync(userId, cancellationToken).ConfigureAwait(false);
    }
}
