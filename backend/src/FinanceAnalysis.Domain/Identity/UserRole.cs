namespace FinanceAnalysis.Domain.Identity;

/// <summary>
/// Join entity between <see cref="User"/> and <see cref="Role"/>.
/// </summary>
public sealed class UserRole
{
    private UserRole()
    {
    }

    public UserRole(Guid userId, int roleId)
    {
        UserId = userId;
        RoleId = roleId;
        AssignedAt = DateTimeOffset.UtcNow;
    }

    public Guid UserId { get; private set; }

    public int RoleId { get; private set; }

    public DateTimeOffset AssignedAt { get; private set; }

    public User User { get; private set; } = null!;

    public Role Role { get; private set; } = null!;
}
