using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Domain.Exceptions;

namespace FinanceAnalysis.Api.Security;

/// <summary>
/// Reads the caller's identity out of the current request's principal.
/// </summary>
/// <remarks>
/// Services depend on <see cref="ICurrentUser"/> rather than <c>IHttpContextAccessor</c> so
/// they stay testable and free of an ASP.NET Core dependency.
/// </remarks>
internal sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;

    public Guid RequireUserId() =>
        UserId ?? throw new UnauthorizedException("The request is not associated with an authenticated user.");
}
