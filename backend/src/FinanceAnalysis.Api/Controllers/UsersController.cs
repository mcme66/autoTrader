using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Application.Features.Authentication;
using FinanceAnalysis.Application.Features.Users;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAnalysis.Api.Controllers;

/// <summary>The signed-in user's own profile.</summary>
[ApiController]
[Route("api/users")]
[Authorize]
[Produces("application/json")]
public sealed class UsersController(IUserService users, ICurrentUser currentUser) : ControllerBase
{
    /// <summary>Returns the caller's profile.</summary>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<AuthenticatedUser> GetCurrentAsync(CancellationToken cancellationToken) =>
        users.GetProfileAsync(currentUser.RequireUserId(), cancellationToken);

    /// <summary>Updates the caller's profile.</summary>
    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<AuthenticatedUser> UpdateCurrentAsync(
        UpdateProfileRequest request,
        CancellationToken cancellationToken) =>
        users.UpdateProfileAsync(currentUser.RequireUserId(), request, cancellationToken);
}
