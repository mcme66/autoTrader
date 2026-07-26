namespace FinanceAnalysis.Domain.Exceptions;

/// <summary>
/// Base type for errors caused by violating a business rule, as opposed to a bug or an
/// infrastructure failure. The API translates these into 4xx responses.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message)
        : base(message)
    {
    }

    protected DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
