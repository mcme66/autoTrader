using FinanceAnalysis.Domain.Common;

namespace FinanceAnalysis.Domain.Identity;

/// <summary>
/// A single-use refresh token. Only the hash is persisted, so a database leak does not
/// hand an attacker usable credentials. Rotation is recorded via
/// <see cref="ReplacedByTokenHash"/> to make token-reuse detection possible.
/// </summary>
public sealed class RefreshToken : Entity<Guid>
{
    private RefreshToken()
    {
    }

    public RefreshToken(Guid userId, string tokenHash, DateTimeOffset expiresAt, string? createdByIp)
    {
        Id = SequentialGuid.New();
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = DateTimeOffset.UtcNow;
        CreatedByIp = createdByIp;
    }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = null!;

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public string? CreatedByIp { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public string? ReplacedByTokenHash { get; private set; }

    public User User { get; private set; } = null!;

    public bool IsRevoked => RevokedAt is not null;

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

    public bool IsActive(DateTimeOffset now) => !IsRevoked && !IsExpired(now);

    public void Revoke(DateTimeOffset now, string? replacedByTokenHash = null)
    {
        if (IsRevoked)
        {
            return;
        }

        RevokedAt = now;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
