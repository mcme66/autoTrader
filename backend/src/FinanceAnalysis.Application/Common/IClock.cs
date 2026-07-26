namespace FinanceAnalysis.Application.Common;

/// <summary>
/// Abstracts the system clock so time-dependent logic (token expiry, trading-day selection)
/// is deterministic under test.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }

    DateOnly UtcToday => DateOnly.FromDateTime(UtcNow.UtcDateTime);
}
