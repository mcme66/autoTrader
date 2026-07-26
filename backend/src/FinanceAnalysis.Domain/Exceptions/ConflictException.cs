namespace FinanceAnalysis.Domain.Exceptions;

/// <summary>
/// Thrown when an operation would violate a uniqueness or state invariant, such as
/// registering an email that already exists. Maps to HTTP 409.
/// </summary>
public sealed class ConflictException : DomainException
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
