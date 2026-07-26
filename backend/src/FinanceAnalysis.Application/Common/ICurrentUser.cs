namespace FinanceAnalysis.Application.Common;

/// <summary>
/// Exposes the authenticated caller to the application layer without dragging in
/// <c>HttpContext</c>.
/// </summary>
public interface ICurrentUser
{
    Guid? UserId { get; }

    bool IsAuthenticated { get; }

    bool IsInRole(string role);

    /// <summary>Returns the caller's id, or throws if the request is anonymous.</summary>
    Guid RequireUserId();
}
