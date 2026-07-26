using FinanceAnalysis.Domain.Common;

namespace FinanceAnalysis.Domain.Identity;

/// <summary>
/// A coarse-grained permission bucket. Two roles ship today (see <see cref="RoleNames"/>);
/// the table exists so finer-grained roles can be added without a schema change.
/// </summary>
public sealed class Role : Entity<int>
{
    private readonly List<UserRole> _userRoles = [];

    private Role()
    {
    }

    public Role(string name, string description)
    {
        Name = name;
        NormalizedName = name.ToUpperInvariant();
        Description = description;
    }

    public string Name { get; private set; } = null!;

    public string NormalizedName { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles;
}
