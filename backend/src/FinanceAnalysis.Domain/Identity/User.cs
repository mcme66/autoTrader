using FinanceAnalysis.Domain.Common;
using FinanceAnalysis.Domain.Portfolios;

namespace FinanceAnalysis.Domain.Identity;

/// <summary>
/// An application account. Local (password) and external (OAuth) credentials are both
/// attached to this entity, so adding an OAuth provider later does not fork the user model.
/// </summary>
public sealed class User : Entity<Guid>, IAuditable
{
    private readonly List<UserRole> _userRoles = [];
    private readonly List<ExternalLogin> _externalLogins = [];
    private readonly List<RefreshToken> _refreshTokens = [];
    private readonly List<Portfolio> _portfolios = [];

    private User()
    {
    }

    private User(string email, string displayName, string? passwordHash)
    {
        Id = SequentialGuid.New();
        Email = email;
        NormalizedEmail = NormalizeEmail(email);
        DisplayName = displayName;
        PasswordHash = passwordHash;
        IsActive = true;
    }

    public string Email { get; private set; } = null!;

    /// <summary>Upper-cased email used for the unique index and for lookups.</summary>
    public string NormalizedEmail { get; private set; } = null!;

    /// <summary>Null for accounts that only authenticate through an external provider.</summary>
    public string? PasswordHash { get; private set; }

    public string DisplayName { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public DateTimeOffset? LastLoginAt { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles;

    public IReadOnlyCollection<ExternalLogin> ExternalLogins => _externalLogins;

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;

    public IReadOnlyCollection<Portfolio> Portfolios => _portfolios;

    public static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    public static User CreateLocal(string email, string displayName, string passwordHash) =>
        new(email.Trim(), displayName.Trim(), passwordHash);

    public static User CreateExternal(string email, string displayName) =>
        new(email.Trim(), displayName.Trim(), passwordHash: null);

    public void AssignRole(Role role)
    {
        if (_userRoles.Exists(x => x.RoleId == role.Id))
        {
            return;
        }

        _userRoles.Add(new UserRole(Id, role.Id));
    }

    public void ChangePassword(string passwordHash) => PasswordHash = passwordHash;

    public void UpdateProfile(string displayName) => DisplayName = displayName.Trim();

    public void RecordLogin(DateTimeOffset at) => LastLoginAt = at;

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    public void LinkExternalLogin(string provider, string providerKey)
    {
        if (_externalLogins.Exists(x => x.Provider == provider && x.ProviderKey == providerKey))
        {
            return;
        }

        _externalLogins.Add(new ExternalLogin(Id, provider, providerKey));
    }
}
