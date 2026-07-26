using FinanceAnalysis.Domain.Common;

namespace FinanceAnalysis.Domain.Identity;

/// <summary>
/// Links a <see cref="User"/> to an identity held by an external provider.
/// Nothing populates this table yet; it exists so that wiring up Google, Microsoft or
/// GitHub sign-in later is additive rather than a migration of the user model.
/// </summary>
public sealed class ExternalLogin : Entity<Guid>
{
    private ExternalLogin()
    {
    }

    public ExternalLogin(Guid userId, string provider, string providerKey)
    {
        Id = SequentialGuid.New();
        UserId = userId;
        Provider = provider;
        ProviderKey = providerKey;
        LinkedAt = DateTimeOffset.UtcNow;
    }

    public Guid UserId { get; private set; }

    /// <summary>Provider discriminator, for example "Google" or "Microsoft".</summary>
    public string Provider { get; private set; } = null!;

    /// <summary>The stable subject identifier issued by the provider.</summary>
    public string ProviderKey { get; private set; } = null!;

    public DateTimeOffset LinkedAt { get; private set; }

    public User User { get; private set; } = null!;
}
