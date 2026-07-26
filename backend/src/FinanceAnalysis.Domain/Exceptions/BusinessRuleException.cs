namespace FinanceAnalysis.Domain.Exceptions;

/// <summary>
/// Thrown when input is well-formed but violates a business rule that request-level
/// validation cannot express. Maps to HTTP 400.
/// </summary>
public sealed class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message)
        : base(message)
    {
    }
}
