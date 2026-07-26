namespace FinanceAnalysis.Application.Abstractions.Ingestion;

/// <summary>
/// Hand-off between the HTTP trigger and the background worker.
/// </summary>
/// <remarks>
/// The trigger endpoint returns <c>202 Accepted</c> immediately because a backfill can run for
/// minutes; the queue is what decouples the two. It is intentionally in-process — the platform
/// has one API instance and durability is provided by the <c>ingestion_runs</c> audit table, so
/// a broker would be unjustified infrastructure. Swapping in a durable queue later means
/// reimplementing this interface and nothing else.
/// </remarks>
public interface IIngestionJobQueue
{
    /// <summary>
    /// Queues <paramref name="job"/>, returning false when the queue is full rather than
    /// blocking the caller. A full queue means triggers are arriving faster than ingestion
    /// completes, which the caller surfaces instead of silently buffering.
    /// </summary>
    bool TryEnqueue(IngestionJob job);

    /// <summary>Yields queued jobs until <paramref name="cancellationToken"/> is signalled.</summary>
    IAsyncEnumerable<IngestionJob> DequeueAllAsync(CancellationToken cancellationToken);
}
