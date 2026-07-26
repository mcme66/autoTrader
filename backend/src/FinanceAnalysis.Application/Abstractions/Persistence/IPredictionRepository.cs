using FinanceAnalysis.Application.Abstractions.Persistence.Projections;
using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Domain.Predictions;

namespace FinanceAnalysis.Application.Abstractions.Persistence;

/// <summary>
/// Read-only access to the tables owned by the external ML pipeline. There is deliberately
/// no write path: this application displays predictions, it does not produce them.
/// </summary>
public interface IPredictionRepository
{
    Task<bool> HasAnyPredictionsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MlModel>> GetModelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// The newest prediction per symbol, optionally restricted to one model.
    /// </summary>
    Task<PagedResult<PredictionSummary>> GetLatestAsync(
        PageRequest page,
        string? modelKey = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PredictionSummary>> GetForSymbolAsync(
        string symbol,
        int limit,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PredictionAccuracy>> GetModelAccuracyAsync(CancellationToken cancellationToken = default);
}
