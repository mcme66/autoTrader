namespace FinanceAnalysis.Domain.Enums;

/// <summary>
/// Direction of the predicted price movement over the prediction horizon.
/// </summary>
public enum PredictionDirection
{
    Unknown = 0,
    Down = 1,
    Flat = 2,
    Up = 3,
}
