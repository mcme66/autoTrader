namespace FinanceAnalysis.Domain.Exceptions;

/// <summary>
/// Thrown when an authenticated caller lacks permission for an operation. Maps to HTTP 403.
/// </summary>
public sealed class ForbiddenException : DomainException
{
    public ForbiddenException(string message)
        : base(message)
    {
    }
}
