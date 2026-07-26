using System.ComponentModel.DataAnnotations;

namespace FinanceAnalysis.Application.Configuration;

/// <summary>
/// Behaviour of the ingestion pipeline, bound from the <c>Ingestion</c> section.
/// </summary>
public sealed class IngestionOptions
{
    public const string SectionName = "Ingestion";

    /// <summary>
    /// Depth of the in-process job queue. Exceeding it means the cron trigger is firing faster
    /// than ingestion completes, which should be visible rather than silently buffered.
    /// </summary>
    [Range(1, 1000)]
    public int QueueCapacity { get; set; } = 32;

    /// <summary>
    /// When true, triggering a day that already ingested successfully is a no-op. Set false to
    /// allow a re-run to attempt symbols that were missing the first time.
    /// </summary>
    public bool SkipAlreadyIngestedDays { get; set; } = true;

    /// <summary>
    /// How far back the daily job looks for the most recent trading day when no explicit date
    /// is supplied. Covers a long weekend plus a public holiday.
    /// </summary>
    [Range(1, 30)]
    public int MaxLookbackDays { get; set; } = 5;

    /// <summary>Whether to apply pending EF Core migrations during startup.</summary>
    public bool ApplyMigrationsOnStartup { get; set; }

    /// <summary>
    /// Whether to reconcile the universe file into the database during startup. Convenient in
    /// containers; off by default so a local run never mutates data unexpectedly.
    /// </summary>
    public bool SyncUniverseOnStartup { get; set; }
}
