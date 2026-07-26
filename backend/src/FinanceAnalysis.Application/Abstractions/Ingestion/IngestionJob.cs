using FinanceAnalysis.Domain.Enums;

namespace FinanceAnalysis.Application.Abstractions.Ingestion;

/// <summary>
/// A unit of ingestion work handed to the background worker.
/// </summary>
/// <remarks>
/// Deliberately a value carrying only identifiers: the job crosses a queue boundary into a
/// different DI scope, so it must not hold entities, repositories, or anything else tied to
/// the request that produced it.
/// </remarks>
/// <param name="RunId">The <c>ingestion_runs</c> row that audits this job.</param>
/// <param name="RunType">What kind of work to perform.</param>
/// <param name="ProviderKey">Provider to collect from, captured at enqueue time.</param>
/// <param name="DataSourceId">Provenance stamped onto every inserted bar.</param>
/// <param name="TradeDate">Target day for <see cref="IngestionRunType.DailyPrices"/>.</param>
/// <param name="RangeStart">Inclusive first day for a backfill.</param>
/// <param name="RangeEnd">Inclusive last day for a backfill.</param>
public sealed record IngestionJob(
    Guid RunId,
    IngestionRunType RunType,
    string ProviderKey,
    int DataSourceId,
    DateOnly? TradeDate = null,
    DateOnly? RangeStart = null,
    DateOnly? RangeEnd = null);
