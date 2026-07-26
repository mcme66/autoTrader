using FinanceAnalysis.Domain.Common;
using FinanceAnalysis.Domain.Enums;

namespace FinanceAnalysis.Domain.MarketData;

/// <summary>
/// An audit record for one ingestion attempt. Because ingestion is triggered by an external
/// cron job and runs asynchronously, this is the only place an operator can see whether
/// yesterday's download actually worked and why it did not.
/// </summary>
public sealed class IngestionRun : Entity<Guid>
{
    private const int ErrorMessageMaxLength = 2000;

    private IngestionRun()
    {
    }

    private IngestionRun(IngestionRunType runType, int dataSourceId, DateOnly? tradeDate)
    {
        Id = SequentialGuid.New();
        RunType = runType;
        DataSourceId = dataSourceId;
        TradeDate = tradeDate;
        Status = IngestionRunStatus.Queued;
        QueuedAt = DateTimeOffset.UtcNow;
    }

    public IngestionRunType RunType { get; private set; }

    public IngestionRunStatus Status { get; private set; }

    public int DataSourceId { get; private set; }

    /// <summary>The trading day being collected. Null for runs that are not day-scoped.</summary>
    public DateOnly? TradeDate { get; private set; }

    public DateOnly? RangeStart { get; private set; }

    public DateOnly? RangeEnd { get; private set; }

    public DateTimeOffset QueuedAt { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public int SymbolsRequested { get; private set; }

    public int SymbolsReceived { get; private set; }

    public int RecordsInserted { get; private set; }

    public int RecordsSkipped { get; private set; }

    public string? ErrorMessage { get; private set; }

    public DataSource DataSource { get; private set; } = null!;

    public TimeSpan? Duration =>
        StartedAt is null || CompletedAt is null ? null : CompletedAt - StartedAt;

    public static IngestionRun ForTradingDay(IngestionRunType runType, int dataSourceId, DateOnly tradeDate) =>
        new(runType, dataSourceId, tradeDate);

    public static IngestionRun ForRange(int dataSourceId, DateOnly rangeStart, DateOnly rangeEnd) =>
        new(IngestionRunType.HistoricalBackfill, dataSourceId, tradeDate: null)
        {
            RangeStart = rangeStart,
            RangeEnd = rangeEnd,
        };

    public static IngestionRun ForUniverseSync(int dataSourceId) =>
        new(IngestionRunType.UniverseSync, dataSourceId, tradeDate: null);

    public void Start(int symbolsRequested)
    {
        Status = IngestionRunStatus.Running;
        StartedAt = DateTimeOffset.UtcNow;
        SymbolsRequested = symbolsRequested;
    }

    public void RecordProgress(int symbolsReceived, int recordsInserted, int recordsSkipped)
    {
        SymbolsReceived += symbolsReceived;
        RecordsInserted += recordsInserted;
        RecordsSkipped += recordsSkipped;
    }

    public void Succeed()
    {
        Status = SymbolsRequested > 0 && SymbolsReceived < SymbolsRequested
            ? IngestionRunStatus.PartiallySucceeded
            : IngestionRunStatus.Succeeded;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Skip(string reason)
    {
        Status = IngestionRunStatus.Skipped;
        ErrorMessage = Truncate(reason);
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Fail(string errorMessage)
    {
        Status = IngestionRunStatus.Failed;
        ErrorMessage = Truncate(errorMessage);
        CompletedAt = DateTimeOffset.UtcNow;
    }

    private static string Truncate(string value) =>
        value.Length <= ErrorMessageMaxLength ? value : value[..ErrorMessageMaxLength];
}
