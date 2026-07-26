namespace FinanceAnalysis.Domain.Common;

/// <summary>
/// Produces time-ordered UUIDv7 identifiers. Sequential keys keep B-tree indexes
/// compact compared with random UUIDv4 values, which matters for the tables that
/// grow indefinitely.
/// </summary>
public static class SequentialGuid
{
    public static Guid New() => Guid.CreateVersion7();
}
