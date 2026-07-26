namespace FinanceAnalysis.Domain.Common;

/// <summary>
/// Marks an entity whose creation and last-modification timestamps are maintained
/// automatically by the persistence layer rather than by callers.
/// </summary>
public interface IAuditable
{
    DateTimeOffset CreatedAt { get; set; }

    DateTimeOffset UpdatedAt { get; set; }
}
