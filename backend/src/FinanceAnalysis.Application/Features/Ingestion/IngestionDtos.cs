using FinanceAnalysis.Domain.Enums;
using FinanceAnalysis.Domain.MarketData;

namespace FinanceAnalysis.Application.Features.Ingestion;

/// <summary>An ingestion attempt as exposed by the internal API.</summary>
public sealed record IngestionRunDto(
    Guid Id,
    IngestionRunType RunType,
    IngestionRunStatus Status,
    string DataSource,
    DateOnly? TradeDate,
    DateOnly? RangeStart,
    DateOnly? RangeEnd,
    DateTimeOffset QueuedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    double? DurationSeconds,
    int SymbolsRequested,
    int SymbolsReceived,
    int RecordsInserted,
    int RecordsSkipped,
    string? ErrorMessage);

internal static class IngestionMappings
{
    public static IngestionRunDto ToDto(this IngestionRun run, string dataSourceKey) => new(
        run.Id,
        run.RunType,
        run.Status,
        dataSourceKey,
        run.TradeDate,
        run.RangeStart,
        run.RangeEnd,
        run.QueuedAt,
        run.StartedAt,
        run.CompletedAt,
        run.Duration?.TotalSeconds,
        run.SymbolsRequested,
        run.SymbolsReceived,
        run.RecordsInserted,
        run.RecordsSkipped,
        run.ErrorMessage);
}
