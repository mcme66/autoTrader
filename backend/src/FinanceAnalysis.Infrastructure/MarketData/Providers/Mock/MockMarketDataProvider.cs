using FinanceAnalysis.Application.Abstractions.MarketData;

namespace FinanceAnalysis.Infrastructure.MarketData.Providers.Mock;

/// <summary>
/// Generates deterministic synthetic bars so the application is fully functional with no API
/// key and no network access.
/// </summary>
/// <remarks>
/// This is what makes <c>docker compose up</c> useful out of the box and what lets integration
/// tests exercise the whole ingestion path without touching a vendor. Prices are derived from a
/// hash of the symbol and date, so the same day always produces the same bar. That determinism
/// matters: re-running an ingestion must be a genuine no-op, and tests must be able to assert
/// on exact values.
/// </remarks>
internal sealed class MockMarketDataProvider : IMarketDataProvider
{
    public const string Key = "mock";

    private const decimal MinBasePrice = 15m;
    private const decimal MaxBasePrice = 650m;

    string IMarketDataProvider.Key => Key;

    public string DisplayName => "Deterministic Mock Provider";

    public Task<DailyBarBatch> GetDailyBarsAsync(
        DateOnly tradeDate,
        IReadOnlySet<string> symbols,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(symbols);

        if (!IsTradingDay(tradeDate))
        {
            return Task.FromResult(DailyBarBatch.MarketClosed(tradeDate));
        }

        var bars = new List<DailyBar>(symbols.Count);

        foreach (var symbol in symbols)
        {
            bars.Add(GenerateBar(symbol, tradeDate));
        }

        return Task.FromResult(new DailyBarBatch(tradeDate, bars));
    }

    public Task<CompanyProfile?> GetCompanyProfileAsync(
        string symbol,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<CompanyProfile?>(new CompanyProfile(
            symbol,
            Description: $"Synthetic profile generated for {symbol} by the mock market data provider.",
            CountryCode: "US"));

    /// <summary>
    /// Weekends only. Exchange holidays are deliberately not modelled: the mock provider exists
    /// to make the pipeline exercisable, not to be a trading calendar.
    /// </summary>
    private static bool IsTradingDay(DateOnly date) =>
        date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);

    private static DailyBar GenerateBar(string symbol, DateOnly tradeDate)
    {
        var basePrice = ScaleToRange(StableHash(symbol), MinBasePrice, MaxBasePrice);

        // A slow sine wave over the day-number gives a series that trends and reverses like a
        // price chart, rather than the white noise a per-day hash alone would produce.
        var dayNumber = tradeDate.DayNumber;
        var wave = (decimal)Math.Sin(dayNumber / 21.0 + StableHash(symbol) % 100 / 15.0);
        var jitter = ScaleToRange(StableHash($"{symbol}:{dayNumber}"), -0.015m, 0.015m);

        var close = Round(basePrice * (1m + (wave * 0.18m) + jitter));
        var open = Round(close * (1m + ScaleToRange(StableHash($"o:{symbol}:{dayNumber}"), -0.012m, 0.012m)));
        var spread = Round(close * ScaleToRange(StableHash($"s:{symbol}:{dayNumber}"), 0.002m, 0.020m));

        var high = Round(Math.Max(open, close) + spread);
        var low = Round(Math.Min(open, close) - spread);
        if (low <= 0m)
        {
            low = Round(Math.Min(open, close) / 2m);
        }

        var volume = 250_000L + (long)(ScaleToRange(StableHash($"v:{symbol}:{dayNumber}"), 0m, 1m) * 40_000_000m);
        var vwap = Round((high + low + close) / 3m);

        return new DailyBar(
            symbol,
            tradeDate,
            open,
            high,
            low,
            close,
            volume,
            vwap,
            (int)(volume / 180));
    }

    private static decimal Round(decimal value) => Math.Round(value, 4, MidpointRounding.AwayFromZero);

    /// <summary>
    /// FNV-1a. <see cref="string.GetHashCode()"/> is randomized per process, which would make
    /// the "deterministic" in this provider's name a lie across restarts.
    /// </summary>
    private static uint StableHash(string value)
    {
        const uint offsetBasis = 2166136261;
        const uint prime = 16777619;

        var hash = offsetBasis;
        foreach (var c in value)
        {
            hash ^= c;
            hash *= prime;
        }

        return hash;
    }

    private static decimal ScaleToRange(uint hash, decimal min, decimal max)
    {
        var unit = (decimal)(hash % 1_000_000u) / 1_000_000m;
        return min + (unit * (max - min));
    }
}
