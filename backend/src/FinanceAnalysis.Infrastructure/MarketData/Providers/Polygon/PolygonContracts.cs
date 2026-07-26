using System.Text.Json.Serialization;

namespace FinanceAnalysis.Infrastructure.MarketData.Providers.Polygon;

/// <summary>
/// Wire contracts for the Polygon.io REST API. Kept internal and separate from the domain so
/// a change on the vendor's side is contained to this file plus the mapping in the provider.
/// </summary>
internal sealed record PolygonGroupedDailyResponse
{
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("resultsCount")]
    public int ResultsCount { get; init; }

    [JsonPropertyName("adjusted")]
    public bool Adjusted { get; init; }

    [JsonPropertyName("results")]
    public IReadOnlyList<PolygonAggregate>? Results { get; init; }
}

internal sealed record PolygonAggregate
{
    /// <summary>Ticker symbol.</summary>
    [JsonPropertyName("T")]
    public string? Ticker { get; init; }

    [JsonPropertyName("o")]
    public decimal Open { get; init; }

    [JsonPropertyName("h")]
    public decimal High { get; init; }

    [JsonPropertyName("l")]
    public decimal Low { get; init; }

    [JsonPropertyName("c")]
    public decimal Close { get; init; }

    [JsonPropertyName("v")]
    public double Volume { get; init; }

    /// <summary>Volume-weighted average price.</summary>
    [JsonPropertyName("vw")]
    public decimal? VolumeWeightedAveragePrice { get; init; }

    /// <summary>Number of transactions in the aggregate window.</summary>
    [JsonPropertyName("n")]
    public int? TransactionCount { get; init; }

    /// <summary>Window start, Unix milliseconds.</summary>
    [JsonPropertyName("t")]
    public long Timestamp { get; init; }
}

internal sealed record PolygonTickerDetailsResponse
{
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("results")]
    public PolygonTickerDetails? Results { get; init; }
}

internal sealed record PolygonTickerDetails
{
    [JsonPropertyName("ticker")]
    public string? Ticker { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("homepage_url")]
    public string? HomepageUrl { get; init; }

    [JsonPropertyName("locale")]
    public string? Locale { get; init; }

    [JsonPropertyName("cik")]
    public string? Cik { get; init; }

    [JsonPropertyName("sic_description")]
    public string? SicDescription { get; init; }

    [JsonPropertyName("total_employees")]
    public int? TotalEmployees { get; init; }

    [JsonPropertyName("list_date")]
    public string? ListDate { get; init; }

    [JsonPropertyName("primary_exchange")]
    public string? PrimaryExchange { get; init; }
}
