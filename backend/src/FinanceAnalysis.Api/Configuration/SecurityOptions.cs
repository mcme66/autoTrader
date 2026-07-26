namespace FinanceAnalysis.Api.Configuration;

/// <summary>
/// Transport-level security settings, bound from the <c>Security</c> section.
/// </summary>
public sealed class SecurityOptions
{
    public const string SectionName = "Security";

    /// <summary>
    /// Shared secret the cron job presents in <c>X-Internal-Api-Key</c>. When unset, the
    /// internal endpoints are unreachable rather than open — failing closed is the only safe
    /// default for an endpoint that can trigger a full historical backfill.
    /// </summary>
    public string? InternalApiKey { get; set; }

    /// <summary>
    /// CIDR ranges permitted to call <c>/api/internal/*</c>. Defaults to loopback and the
    /// RFC 1918 private ranges, which covers both a host cron job and a sibling container.
    /// </summary>
    public IList<string> InternalAllowedNetworks { get; } =
    [
        "127.0.0.1/32",
        "::1/128",
        "10.0.0.0/8",
        "172.16.0.0/12",
        "192.168.0.0/16",
    ];

    /// <summary>
    /// Origins allowed to call the API from a browser. Empty in production, where nginx serves
    /// the SPA and the API from the same origin and CORS is therefore not involved at all.
    /// </summary>
    public IList<string> CorsOrigins { get; } = [];

    /// <summary>
    /// Whether refresh-token cookies carry the <c>Secure</c> flag. On in production; off for
    /// local development over plain HTTP, where a Secure cookie would simply be dropped.
    /// </summary>
    public bool RequireSecureCookies { get; set; } = true;
}
