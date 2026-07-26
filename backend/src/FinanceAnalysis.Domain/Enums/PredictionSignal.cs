namespace FinanceAnalysis.Domain.Enums;

/// <summary>
/// The actionable recommendation attached to a prediction. Written by the external ML
/// pipeline; this application only reads it.
/// </summary>
public enum PredictionSignal
{
    Unknown = 0,
    StrongSell = 1,
    Sell = 2,
    Hold = 3,
    Buy = 4,
    StrongBuy = 5,
}
