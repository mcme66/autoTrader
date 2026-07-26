using System.ComponentModel.DataAnnotations;

namespace FinanceAnalysis.Application.Configuration;

/// <summary>
/// Market data collection settings, bound from the <c>MarketData</c> configuration section.
/// </summary>
public sealed class MarketDataOptions
{
    public const string SectionName = "MarketData";

    /// <summary>
    /// Key of the active provider. Must match a registered <c>IMarketDataProvider.Key</c>.
    /// Defaults to the mock provider so a fresh checkout runs with no API key at all.
    /// </summary>
    [Required]
    public string Provider { get; set; } = "mock";

    /// <summary>Path to the universe file, absolute or relative to the content root.</summary>
    [Required]
    public string UniverseFilePath { get; set; } = "config/universe.json";

    /// <summary>
    /// Hard ceiling on tracked symbols. Guards against a mis-edited universe file quietly
    /// blowing through a provider's rate limit or storage expectations.
    /// </summary>
    [Range(1, 5000)]
    public int MaxTrackedSymbols { get; set; } = 500;

    /// <summary>
    /// How many calendar days a single backfill request may span. Backfills are rate-limited
    /// and long-running, so an unbounded range is a foot-gun.
    /// </summary>
    [Range(1, 3650)]
    public int MaxBackfillDays { get; set; } = 730;

    public PolygonOptions Polygon { get; set; } = new();
}

/// <summary>Settings specific to the Polygon.io provider.</summary>
public sealed class PolygonOptions
{
    /// <summary>
    /// API key. Never committed: supply through the <c>MarketData__Polygon__ApiKey</c>
    /// environment variable in Docker or <c>dotnet user-secrets</c> locally.
    /// </summary>
    public string? ApiKey { get; set; }

    public Uri BaseAddress { get; set; } = new("https://api.polygon.io/");

    /// <summary>
    /// Requests permitted per minute. Polygon's free tier allows five; raise this to match a
    /// paid plan.
    /// </summary>
    [Range(1, 10000)]
    public int RequestsPerMinute { get; set; } = 5;

    /// <summary>Whether to request split- and dividend-adjusted bars.</summary>
    public bool Adjusted { get; set; } = true;

    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;
}
