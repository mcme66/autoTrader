using FinanceAnalysis.Application.Abstractions.MarketData;
using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Application.Configuration;
using FinanceAnalysis.Application.Features.Universe;
using FinanceAnalysis.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FinanceAnalysis.Api.Extensions;

/// <summary>
/// Startup work that needs a service scope: migrations, optional universe seeding, and the
/// banner that tells an operator what this process actually is.
/// </summary>
internal static class ApplicationInitializer
{
    public static async Task InitializeAsync(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        await using var scope = app.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var ingestion = services.GetRequiredService<IOptions<IngestionOptions>>().Value;
        var marketData = services.GetRequiredService<IOptions<MarketDataOptions>>().Value;
        var db = services.GetRequiredService<ApplicationDbContext>();

        var pending = await ApplyMigrationsAsync(db, ingestion, logger);

        if (ingestion.SyncUniverseOnStartup && pending == 0)
        {
            await SyncUniverseAsync(services, logger);
        }

        await LogStartupBannerAsync(app, services, marketData, logger);
    }

    /// <summary>
    /// Applies migrations when configured to. Off by default for local development, where an
    /// unexpected schema change during a debug session is worse than an explicit
    /// <c>dotnet ef database update</c>; on in containers, where nothing else can run them.
    /// </summary>
    private static async Task<int> ApplyMigrationsAsync(
        ApplicationDbContext db,
        IngestionOptions options,
        ILogger logger)
    {
        var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();

        if (pending.Count == 0)
        {
            return 0;
        }

        if (!options.ApplyMigrationsOnStartup)
        {
            logger.LogWarning(
                "{Count} database migration(s) are pending and automatic migration is disabled. "
                + "Run 'dotnet ef database update' or set Ingestion:ApplyMigrationsOnStartup=true.",
                pending.Count);

            return pending.Count;
        }

        logger.LogInformation("Applying {Count} pending database migration(s).", pending.Count);
        await db.Database.MigrateAsync();
        logger.LogInformation("Database schema is up to date.");

        return 0;
    }

    private static async Task SyncUniverseAsync(IServiceProvider services, ILogger logger)
    {
        try
        {
            var result = await services.GetRequiredService<IUniverseSyncService>().SyncAsync();
            logger.LogInformation(
                "Universe seeded on startup: {Total} symbols declared, {Added} added.",
                result.SymbolsInFile,
                result.Added);
        }
#pragma warning disable CA1031 // A seeding failure must not prevent the API from serving requests.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            logger.LogError(ex, "Universe sync on startup failed. The API will start with the existing catalogue.");
        }
    }

    private static async Task LogStartupBannerAsync(
        WebApplication app,
        IServiceProvider services,
        MarketDataOptions marketData,
        ILogger logger)
    {
        var providers = services.GetRequiredService<IMarketDataProviderResolver>();
        var trackedCount = await SafeCountAsync(services, logger);

        logger.LogInformation(
            "Finance Analysis Platform API starting. Environment: {Environment}. "
            + "Market data provider: {Provider} (available: {AvailableProviders}). "
            + "Tracked symbols: {TrackedSymbols}. Universe file: {UniverseFile}.",
            app.Environment.EnvironmentName,
            marketData.Provider,
            string.Join(", ", providers.AvailableKeys),
            trackedCount,
            marketData.UniverseFilePath);
    }

    private static async Task<string> SafeCountAsync(IServiceProvider services, ILogger logger)
    {
        try
        {
            var count = await services.GetRequiredService<IStockRepository>().CountTrackedAsync();
            return count.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
#pragma warning disable CA1031 // The banner is diagnostics; an unreachable database is reported by health checks.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            logger.LogWarning(ex, "Could not read the tracked symbol count during startup.");
            return "unavailable";
        }
    }
}
