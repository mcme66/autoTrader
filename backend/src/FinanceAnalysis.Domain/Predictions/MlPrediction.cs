using FinanceAnalysis.Domain.Catalog;
using FinanceAnalysis.Domain.Enums;

namespace FinanceAnalysis.Domain.Predictions;

/// <summary>
/// The current prediction for one symbol from one model.
/// </summary>
/// <remarks>
/// Owned by the external ML pipeline and read-only here. Every field is nullable because
/// different model families emit different outputs: a classifier may set only
/// <see cref="Direction"/> and <see cref="Signal"/>, while a regressor sets
/// <see cref="PredictedClose"/>. The UI renders whatever is present.
/// </remarks>
public sealed class MlPrediction
{
    private readonly List<MlPredictionHistory> _history = [];

    private MlPrediction()
    {
    }

    public long Id { get; private set; }

    public int ModelId { get; private set; }

    public int StockId { get; private set; }

    /// <summary>The date the prediction was generated.</summary>
    public DateOnly PredictionDate { get; private set; }

    /// <summary>The date the prediction is about.</summary>
    public DateOnly TargetDate { get; private set; }

    /// <summary>Number of trading days between prediction and target.</summary>
    public int HorizonDays { get; private set; }

    public decimal? PredictedClose { get; private set; }

    public decimal? PredictedReturn { get; private set; }

    public PredictionDirection Direction { get; private set; }

    public PredictionSignal Signal { get; private set; }

    /// <summary>Model confidence in the range 0 to 1, when the model reports one.</summary>
    public decimal? Confidence { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public MlModel Model { get; private set; } = null!;

    public Stock Stock { get; private set; } = null!;

    public IReadOnlyCollection<MlPredictionHistory> History => _history;
}
