namespace FinanceAnalysis.Domain.Exceptions;

/// <summary>
/// Thrown when credentials are missing, wrong or no longer valid. Maps to HTTP 401.
/// </summary>
public sealed class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message)
        : base(message)
    {
    }
}
