using FinanceAnalysis.Application.Abstractions.Ingestion;

namespace FinanceAnalysis.Application.Features.Ingestion;

/// <summary>
/// Performs queued ingestion work. Invoked by the background worker inside its own DI scope,
/// never directly from a request.
/// </summary>
public interface IIngestionExecutor
{
    Task ExecuteAsync(IngestionJob job, CancellationToken cancellationToken = default);
}
