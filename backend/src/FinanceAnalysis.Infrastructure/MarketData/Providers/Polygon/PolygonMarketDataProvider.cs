using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FinanceAnalysis.Application.Abstractions.MarketData;
using FinanceAnalysis.Application.Configuration;
using FinanceAnalysis.Domain.Exceptions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceAnalysis.Infrastructure.MarketData.Providers.Polygon;

/// <summary>
/// Polygon.io implementation of <see cref="IMarketDataProvider"/>.
/// </summary>
/// <remarks>
/// Uses the grouped-daily endpoint, which returns every US equity for a date in a single
/// response. That matters on the free tier: collecting the whole 300-symbol universe costs one
/// request out of five per minute, where a per-symbol endpoint would need an hour. Rate
/// limiting and retries are applied by handlers configured in <c>DependencyInjection</c>, so
/// this class only deals with mapping.
/// </remarks>
internal sealed class PolygonMarketDataProvider(
    HttpClient httpClient,
    IOptions<MarketDataOptions> options,
    ILogger<PolygonMarketDataProvider> logger) : IMarketDataProvider
{
    public const string Key = "polygon";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly PolygonOptions _options = options.Value.Polygon;

    string IMarketDataProvider.Key => Key;

    public string DisplayName => "Polygon.io";

    public async Task<DailyBarBatch> GetDailyBarsAsync(
        DateOnly tradeDate,
        IReadOnlySet<string> symbols,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        EnsureConfigured();

        var date = tradeDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var adjusted = _options.Adjusted ? "true" : "false";
        var requestUri = $"v2/aggs/grouped/locale/us/market/stocks/{date}?adjusted={adjusted}&include_otc=false";

        using var response = await httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new BusinessRuleException(
                "Polygon rejected the API key. Check the MarketData:Polygon:ApiKey setting.");
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new BusinessRuleException(
                "Polygon returned 429. Lower MarketData:Polygon:RequestsPerMinute to match your plan.");
        }

        response.EnsureSuccessStatusCode();

        var payload = await response.Content
            .ReadFromJsonAsync<PolygonGroupedDailyResponse>(SerializerOptions, cancellationToken)
            .ConfigureAwait(false);

        // Polygon answers 200 with an empty result set for weekends and market holidays.
        if (payload?.Results is null or { Count: 0 })
        {
            logger.LogInformation(
                "Polygon returned no aggregates for {TradeDate}; treating it as a non-trading day.",
                date);

            return DailyBarBatch.MarketClosed(tradeDate);
        }

        var bars = new List<DailyBar>(symbols.Count);
        var skipped = 0;

        foreach (var aggregate in payload.Results)
        {
            if (aggregate.Ticker is null || !symbols.Contains(aggregate.Ticker))
            {
                continue;
            }

            var bar = new DailyBar(
                aggregate.Ticker,
                tradeDate,
                aggregate.Open,
                aggregate.High,
                aggregate.Low,
                aggregate.Close,
                (long)Math.Round(aggregate.Volume, MidpointRounding.AwayFromZero),
                aggregate.VolumeWeightedAveragePrice,
                aggregate.TransactionCount);

            if (bar.IsValid())
            {
                bars.Add(bar);
            }
            else
            {
                skipped++;
            }
        }

        if (skipped > 0)
        {
            logger.LogWarning(
                "Discarded {Skipped} malformed Polygon bars for {TradeDate}.",
                skipped,
                date);
        }

        logger.LogInformation(
            "Polygon returned {Matched} of {Requested} tracked symbols for {TradeDate} "
            + "(out of {Total} results in the response).",
            bars.Count,
            symbols.Count,
            date,
            payload.Results.Count);

        return new DailyBarBatch(tradeDate, bars);
    }

    public async Task<CompanyProfile?> GetCompanyProfileAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        using var response = await httpClient
            .GetAsync($"v3/reference/tickers/{Uri.EscapeDataString(symbol)}", cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var payload = await response.Content
            .ReadFromJsonAsync<PolygonTickerDetailsResponse>(SerializerOptions, cancellationToken)
            .ConfigureAwait(false);

        var details = payload?.Results;
        if (details is null)
        {
            return null;
        }

        return new CompanyProfile(
            symbol,
            details.Name,
            details.Description,
            details.HomepageUrl,
            NormalizeCountry(details.Locale),
            details.Cik,
            details.SicDescription,
            details.TotalEmployees,
            ParseDate(details.ListDate));
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new BusinessRuleException(
                "The Polygon provider is selected but no API key is configured. Set "
                + "MarketData__Polygon__ApiKey, or set MarketData:Provider to 'mock' to run without a key.");
        }
    }

    private static string? NormalizeCountry(string? locale) =>
        string.IsNullOrWhiteSpace(locale) ? null : locale.Trim().ToUpperInvariant()[..Math.Min(2, locale.Trim().Length)];

    private static DateOnly? ParseDate(string? value) =>
        DateOnly.TryParse(value, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
}
