using FinanceAnalysis.Domain.Enums;

namespace FinanceAnalysis.Application.Abstractions.Persistence.Projections;

/// <summary>
/// A prediction joined to the symbol and model it belongs to. Read-only: rows originate
/// from the external ML pipeline.
/// </summary>
public sealed record PredictionSummary(
    long PredictionId,
    string Symbol,
    string CompanyName,
    string? SectorName,
    string ModelKey,
    string ModelName,
    string ModelVersion,
    DateOnly PredictionDate,
    DateOnly TargetDate,
    int HorizonDays,
    decimal? PredictedClose,
    decimal? PredictedReturn,
    PredictionDirection Direction,
    PredictionSignal Signal,
    decimal? Confidence,
    decimal? LatestClose);
