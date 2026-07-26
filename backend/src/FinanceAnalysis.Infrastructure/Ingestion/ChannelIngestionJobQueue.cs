using System.Threading.Channels;

using FinanceAnalysis.Application.Abstractions.Ingestion;
using FinanceAnalysis.Application.Configuration;

using Microsoft.Extensions.Options;

namespace FinanceAnalysis.Infrastructure.Ingestion;

/// <summary>
/// Bounded in-memory job queue backed by <see cref="Channel{T}"/>.
/// </summary>
/// <remarks>
/// <see cref="BoundedChannelFullMode.Wait"/> combined with <c>TryWrite</c> gives a non-blocking
/// rejection when the queue is saturated: the trigger fails loudly instead of the API silently
/// accumulating work it cannot keep up with.
/// </remarks>
internal sealed class ChannelIngestionJobQueue : IIngestionJobQueue
{
    private readonly Channel<IngestionJob> _channel;

    public ChannelIngestionJobQueue(IOptions<IngestionOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _channel = Channel.CreateBounded<IngestionJob>(new BoundedChannelOptions(options.Value.QueueCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        });
    }

    public bool TryEnqueue(IngestionJob job) => _channel.Writer.TryWrite(job);

    public IAsyncEnumerable<IngestionJob> DequeueAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
