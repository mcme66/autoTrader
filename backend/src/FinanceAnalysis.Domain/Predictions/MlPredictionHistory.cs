using FinanceAnalysis.Domain.Catalog;

namespace FinanceAnalysis.Domain.Predictions;

/// <summary>
/// A scored prediction: what the model said, next to what actually happened.
/// </summary>
/// <remarks>
/// Owned by the external ML pipeline and read-only here. This is what makes per-model
/// accuracy reporting possible once the pipeline starts back-scoring its own output.
/// </remarks>
public sealed class MlPredictionHistory
{
    private MlPredictionHistory()
    {
    }

    public long Id { get; private set; }

    public long PredictionId { get; private set; }

    public int ModelId { get; private set; }

    public int StockId { get; private set; }

    public DateOnly TargetDate { get; private set; }

    public decimal? PredictedValue { get; private set; }

    public decimal? ActualValue { get; private set; }

    public decimal? AbsoluteError { get; private set; }

    public decimal? PercentageError { get; private set; }

    /// <summary>Whether the predicted direction matched the realised direction.</summary>
    public bool? DirectionCorrect { get; private set; }

    public DateTimeOffset EvaluatedAt { get; private set; }

    public MlPrediction Prediction { get; private set; } = null!;

    public MlModel Model { get; private set; } = null!;

    public Stock Stock { get; private set; } = null!;
}
