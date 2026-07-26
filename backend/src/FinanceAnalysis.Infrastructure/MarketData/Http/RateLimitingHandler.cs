using System.Threading.RateLimiting;

using Microsoft.Extensions.Logging;

namespace FinanceAnalysis.Infrastructure.MarketData.Http;

/// <summary>
/// Client-side throttle for an outbound API.
/// </summary>
/// <remarks>
/// Sits in front of the resilience handler so retries are throttled too. Without this, a
/// backfill would burn through a free-tier quota in seconds and then spend the rest of the run
/// retrying 429s. Waiting for a token is strictly better than being rejected and retrying.
/// </remarks>
internal sealed class RateLimitingHandler : DelegatingHandler
{
    private readonly RateLimiter _limiter;
    private readonly ILogger<RateLimitingHandler> _logger;

    public RateLimitingHandler(int requestsPerMinute, ILogger<RateLimitingHandler> logger)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(requestsPerMinute, 1);

        _logger = logger;
        _limiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = requestsPerMinute,
            TokensPerPeriod = requestsPerMinute,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            QueueLimit = int.MaxValue,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            AutoReplenishment = true,
        });
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using var lease = await _limiter.AcquireAsync(1, cancellationToken).ConfigureAwait(false);

        if (!lease.IsAcquired)
        {
            _logger.LogWarning(
                "Rate limiter refused a lease for {Uri}; the request was not sent.",
                request.RequestUri);

            return new HttpResponseMessage(System.Net.HttpStatusCode.TooManyRequests)
            {
                RequestMessage = request,
                ReasonPhrase = "Client-side rate limit exhausted.",
            };
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _limiter.Dispose();
        }

        base.Dispose(disposing);
    }
}
