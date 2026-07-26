using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Application.Abstractions.Persistence.Projections;
using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Domain.Enums;

namespace FinanceAnalysis.Application.Features.Recommendations;

/// <summary>A model prediction as presented to the UI.</summary>
public sealed record RecommendationDto(
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
    decimal? LatestClose,
    decimal? ImpliedUpsidePercent);

/// <summary>A model registered by the external pipeline.</summary>
public sealed record MlModelDto(
    string Key,
    string Name,
    string Version,
    string? Description,
    bool IsActive,
    DateTimeOffset CreatedAt);

/// <summary>Back-scored accuracy for one model.</summary>
public sealed record ModelAccuracyDto(
    string ModelKey,
    string ModelName,
    string ModelVersion,
    int EvaluatedCount,
    decimal? MeanAbsoluteError,
    decimal? MeanAbsolutePercentageError,
    decimal? DirectionalAccuracyPercent);

/// <summary>
/// The recommendations screen payload.
/// </summary>
/// <remarks>
/// <see cref="HasPredictions"/> is what the frontend keys its empty state off. It is a distinct
/// signal from "this page of results happens to be empty", because an untrained pipeline and a
/// filter that matched nothing warrant different messages.
/// </remarks>
public sealed record RecommendationsDto(
    bool HasPredictions,
    PagedResult<RecommendationDto> Predictions,
    IReadOnlyList<MlModelDto> Models,
    IReadOnlyList<ModelAccuracyDto> Accuracy);

public interface IRecommendationService
{
    Task<RecommendationsDto> GetLatestAsync(
        PageRequest page,
        string? modelKey = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecommendationDto>> GetForSymbolAsync(
        string symbol,
        int limit = 20,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Reads predictions produced by MLPipeline_Jordan.
/// </summary>
/// <remarks>
/// Strictly read-only. This application never writes to the ML tables; if these endpoints
/// return nothing it is because the pipeline has not run, which is a valid and expected state
/// that the UI renders as a placeholder rather than an error.
/// </remarks>
public sealed class RecommendationService(IPredictionRepository predictions) : IRecommendationService
{
    private const int MaxSymbolHistory = 100;

    public async Task<RecommendationsDto> GetLatestAsync(
        PageRequest page,
        string? modelKey = null,
        CancellationToken cancellationToken = default)
    {
        var hasAny = await predictions.HasAnyPredictionsAsync(cancellationToken).ConfigureAwait(false);

        if (!hasAny)
        {
            return new RecommendationsDto(
                HasPredictions: false,
                PagedResult.Empty<RecommendationDto>(page.Page, page.PageSize),
                [],
                []);
        }

        var latest = await predictions.GetLatestAsync(page, modelKey, cancellationToken).ConfigureAwait(false);
        var models = await predictions.GetModelsAsync(cancellationToken).ConfigureAwait(false);
        var accuracy = await predictions.GetModelAccuracyAsync(cancellationToken).ConfigureAwait(false);

        return new RecommendationsDto(
            HasPredictions: true,
            latest.Map(ToDto),
            [
                .. models.Select(m => new MlModelDto(
                    m.Key,
                    m.Name,
                    m.Version,
                    m.Description,
                    m.IsActive,
                    m.CreatedAt)),
            ],
            [
                .. accuracy.Select(a => new ModelAccuracyDto(
                    a.ModelKey,
                    a.ModelName,
                    a.ModelVersion,
                    a.EvaluatedCount,
                    Round(a.MeanAbsoluteError),
                    Round(a.MeanAbsolutePercentageError),
                    Round(a.DirectionalAccuracy))),
            ]);
    }

    public async Task<IReadOnlyList<RecommendationDto>> GetForSymbolAsync(
        string symbol,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var results = await predictions
            .GetForSymbolAsync(symbol, Math.Clamp(limit, 1, MaxSymbolHistory), cancellationToken)
            .ConfigureAwait(false);

        return [.. results.Select(ToDto)];
    }

    private static RecommendationDto ToDto(PredictionSummary p) => new(
        p.PredictionId,
        p.Symbol,
        p.CompanyName,
        p.SectorName,
        p.ModelKey,
        p.ModelName,
        p.ModelVersion,
        p.PredictionDate,
        p.TargetDate,
        p.HorizonDays,
        p.PredictedClose,
        p.PredictedReturn,
        p.Direction,
        p.Signal,
        p.Confidence,
        p.LatestClose,
        p.PredictedClose is null || p.LatestClose is null or 0m
            ? null
            : Math.Round((p.PredictedClose.Value - p.LatestClose.Value) / p.LatestClose.Value * 100m, 4));

    private static decimal? Round(decimal? value) =>
        value is null ? null : Math.Round(value.Value, 4, MidpointRounding.AwayFromZero);
}
