namespace FinanceAnalysis.Domain.Enums;

/// <summary>
/// Distinguishes the kinds of work the ingestion pipeline performs so that runs can be
/// filtered and rate-limited independently.
/// </summary>
public enum IngestionRunType
{
    DailyPrices = 0,
    HistoricalBackfill = 1,
    UniverseSync = 2,
    CompanyProfiles = 3,
}
