namespace FinanceAnalysis.Application.Abstractions.Persistence.Projections;

/// <summary>
/// Back-scored accuracy for one model, aggregated from prediction history.
/// </summary>
public sealed record PredictionAccuracy(
    string ModelKey,
    string ModelName,
    string ModelVersion,
    int EvaluatedCount,
    decimal? MeanAbsoluteError,
    decimal? MeanAbsolutePercentageError,
    decimal? DirectionalAccuracy);
