using FinanceAnalysis.Application.Abstractions.Ingestion;
using FinanceAnalysis.Application.Features.Ingestion;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinanceAnalysis.Infrastructure.Ingestion;

/// <summary>
/// Drains the ingestion queue, running one job at a time.
/// </summary>
/// <remarks>
/// Serial execution is deliberate: the provider's rate limit is the bottleneck, so running
/// jobs concurrently would only trade queueing delay for throttling delay while making the
/// audit trail harder to read. Each job gets its own DI scope because the executor depends on
/// scoped repositories and a scoped <c>DbContext</c>, neither of which may be shared across
/// jobs or captured by this singleton.
/// </remarks>
internal sealed partial class IngestionBackgroundService(
    IIngestionJobQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<IngestionBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogWorkerStarted(logger);

        try
        {
            await foreach (var job in queue.DequeueAllAsync(stoppingToken).ConfigureAwait(false))
            {
                await RunJobAsync(job, stoppingToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal shutdown.
        }

        LogWorkerStopped(logger);
    }

    private async Task RunJobAsync(IngestionJob job, CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var executor = scope.ServiceProvider.GetRequiredService<IIngestionExecutor>();

        try
        {
            await executor.ExecuteAsync(job, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
#pragma warning disable CA1031 // The executor already records failures; this is the last line of defence for the loop.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            LogJobFaulted(logger, ex, job.RunId);
        }
    }

    [LoggerMessage(EventId = 2001, Level = LogLevel.Information, Message = "Ingestion worker started.")]
    private static partial void LogWorkerStarted(ILogger logger);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Information, Message = "Ingestion worker stopped.")]
    private static partial void LogWorkerStopped(ILogger logger);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Critical,
        Message = "Ingestion job for run {RunId} escaped its own error handling.")]
    private static partial void LogJobFaulted(ILogger logger, Exception exception, Guid runId);
}
