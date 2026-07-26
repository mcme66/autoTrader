namespace FinanceAnalysis.Domain.Enums;

/// <summary>
/// Lifecycle of a single ingestion attempt.
/// </summary>
public enum IngestionRunStatus
{
    Queued = 0,
    Running = 1,
    Succeeded = 2,
    PartiallySucceeded = 3,
    Failed = 4,
    Skipped = 5,
}
