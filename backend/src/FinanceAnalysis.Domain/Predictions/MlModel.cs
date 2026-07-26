using FinanceAnalysis.Domain.Common;

namespace FinanceAnalysis.Domain.Predictions;

/// <summary>
/// A model registered by the external ML pipeline.
/// </summary>
/// <remarks>
/// Owned by MLPipeline_Jordan. This application creates the table through its migrations so
/// the pipeline has a schema to write into, but treats the rows as read-only: there are no
/// mutators here and no write repository anywhere in the codebase. Several models can be
/// active at once, which is why predictions reference a model rather than standing alone.
/// </remarks>
public sealed class MlModel : Entity<int>
{
    private readonly List<MlPrediction> _predictions = [];

    private MlModel()
    {
    }

    /// <summary>Stable identifier for the model family, for example "lstm-close-5d".</summary>
    public string Key { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string Version { get; private set; } = null!;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<MlPrediction> Predictions => _predictions;
}
