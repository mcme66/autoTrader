namespace FinanceAnalysis.Domain.Exceptions;

/// <summary>
/// Thrown when a requested resource does not exist, or exists but is not visible to the
/// caller. Maps to HTTP 404.
/// </summary>
public sealed class NotFoundException : DomainException
{
    public NotFoundException(string resource, object key)
        : base($"{resource} '{key}' was not found.")
    {
        Resource = resource;
        Key = key;
    }

    public NotFoundException(string message)
        : base(message)
    {
        Resource = string.Empty;
        Key = string.Empty;
    }

    public string Resource { get; }

    public object Key { get; }
}
