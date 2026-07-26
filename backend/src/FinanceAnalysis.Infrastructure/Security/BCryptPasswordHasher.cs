using FinanceAnalysis.Application.Abstractions.Security;
using FinanceAnalysis.Application.Configuration;

using Microsoft.Extensions.Options;

namespace FinanceAnalysis.Infrastructure.Security;

/// <summary>
/// BCrypt password hashing.
/// </summary>
/// <remarks>
/// BCrypt embeds its salt and cost factor in the hash string, so raising
/// <c>Auth:PasswordHashWorkFactor</c> takes effect on the next sign-in for each user via
/// <see cref="NeedsRehash"/> rather than requiring a migration or a forced password reset.
/// </remarks>
internal sealed class BCryptPasswordHasher(IOptions<AuthenticationOptions> options) : IPasswordHasher
{
    private readonly int _workFactor = options.Value.PasswordHashWorkFactor;

    public string Hash(string password) =>
        BCrypt.Net.BCrypt.EnhancedHashPassword(password, _workFactor);

    public bool Verify(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // A malformed stored hash is a failed verification, not a crash.
            return false;
        }
    }

    public bool NeedsRehash(string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.PasswordNeedsRehash(passwordHash, _workFactor);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return true;
        }
    }
}
