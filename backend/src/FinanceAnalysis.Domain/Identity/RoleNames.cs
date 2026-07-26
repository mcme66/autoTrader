namespace FinanceAnalysis.Domain.Identity;

/// <summary>
/// The role names seeded at migration time and referenced by authorization policies.
/// </summary>
public static class RoleNames
{
    public const string Administrator = "Administrator";

    public const string Member = "Member";

    public static IReadOnlyList<string> All { get; } = [Administrator, Member];
}
