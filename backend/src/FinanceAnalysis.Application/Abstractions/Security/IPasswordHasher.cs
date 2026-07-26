namespace FinanceAnalysis.Application.Abstractions.Security;

/// <summary>
/// Password hashing, abstracted so the algorithm can be replaced without touching the
/// authentication service.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    /// <summary>
    /// Verifies a password against a stored hash. Implementations must be constant-time with
    /// respect to the comparison to avoid leaking information through timing.
    /// </summary>
    bool Verify(string password, string passwordHash);

    /// <summary>
    /// True when the stored hash used a weaker work factor than the current configuration,
    /// signalling that it should be re-hashed on the next successful sign-in.
    /// </summary>
    bool NeedsRehash(string passwordHash);
}
