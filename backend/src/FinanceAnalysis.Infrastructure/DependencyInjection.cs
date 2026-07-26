using FinanceAnalysis.Application.Abstractions.Ingestion;
using FinanceAnalysis.Application.Abstractions.MarketData;
using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Application.Abstractions.Security;
using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Application.Configuration;
using FinanceAnalysis.Infrastructure.Common;
using FinanceAnalysis.Infrastructure.Ingestion;
using FinanceAnalysis.Infrastructure.MarketData;
using FinanceAnalysis.Infrastructure.MarketData.Http;
using FinanceAnalysis.Infrastructure.MarketData.Providers.Mock;
using FinanceAnalysis.Infrastructure.MarketData.Providers.Polygon;
using FinanceAnalysis.Infrastructure.MarketData.Universe;
using FinanceAnalysis.Infrastructure.Persistence;
using FinanceAnalysis.Infrastructure.Persistence.Interceptors;
using FinanceAnalysis.Infrastructure.Persistence.Repositories;
using FinanceAnalysis.Infrastructure.Security;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceAnalysis.Infrastructure;

/// <summary>
/// Wires the infrastructure implementations behind the application's abstractions.
/// </summary>
/// <remarks>
/// This is the only place that knows about PostgreSQL, HTTP, and specific vendors. The API's
/// composition root calls it once; nothing else in the solution references these types.
/// </remarks>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>Connection string name expected in configuration.</summary>
    public const string ConnectionStringName = "DefaultConnection";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddConfiguredOptions(configuration)
            .AddPersistence(configuration)
            .AddSecurity()
            .AddMarketData()
            .AddIngestion();

        services.AddSingleton<IClock, SystemClock>();

        return services;
    }

    /// <summary>
    /// Binds and validates every options object at startup, so a typo in configuration fails
    /// the process immediately rather than surfacing as a confusing error hours later during
    /// the nightly ingest.
    /// </summary>
    private static IServiceCollection AddConfiguredOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<MarketDataOptions>()
            .Bind(configuration.GetSection(MarketDataOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<IngestionOptions>()
            .Bind(configuration.GetSection(IngestionOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<AuthenticationOptions>()
            .Bind(configuration.GetSection(AuthenticationOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is not configured.");

        services.AddSingleton<AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
            options
                .UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(
                    typeof(ApplicationDbContext).Assembly.FullName))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ISectorRepository, SectorRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<IDailyPriceRepository, DailyPriceRepository>();
        services.AddScoped<IDataSourceRepository, DataSourceRepository>();
        services.AddScoped<IIngestionRunRepository, IngestionRunRepository>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<IPredictionRepository, PredictionRepository>();
        services.AddScoped<IMarketOverviewRepository, MarketOverviewRepository>();

        return services;
    }

    private static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }

    private static IServiceCollection AddMarketData(this IServiceCollection services)
    {
        services.AddSingleton<IStockUniverseSource, JsonFileStockUniverseSource>();

        services.AddKeyedSingleton<IMarketDataProvider, MockMarketDataProvider>(MockMarketDataProvider.Key);

        // A typed client rather than a bare HttpClient: the rate limiter must live for the
        // lifetime of the handler chain, and pooled handlers give us that without socket
        // exhaustion. Order matters — throttle first, then retry, so retries are throttled too.
        services.AddHttpClient<PolygonMarketDataProvider>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<MarketDataOptions>>().Value.Polygon;
                client.BaseAddress = options.BaseAddress;
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .AddHttpMessageHandler(sp =>
            {
                var options = sp.GetRequiredService<IOptions<MarketDataOptions>>().Value.Polygon;
                return new RateLimitingHandler(
                    options.RequestsPerMinute,
                    sp.GetRequiredService<ILogger<RateLimitingHandler>>());
            })
            .AddStandardResilienceHandler();

        services.AddKeyedScoped<IMarketDataProvider>(
            PolygonMarketDataProvider.Key,
            (sp, _) => sp.GetRequiredService<PolygonMarketDataProvider>());

        services.AddSingleton(new MarketDataProviderRegistry(
            [MockMarketDataProvider.Key, PolygonMarketDataProvider.Key]));
        services.AddScoped<IMarketDataProviderResolver, MarketDataProviderResolver>();

        return services;
    }

    private static IServiceCollection AddIngestion(this IServiceCollection services)
    {
        services.AddSingleton<IIngestionJobQueue, ChannelIngestionJobQueue>();
        services.AddHostedService<IngestionBackgroundService>();

        return services;
    }
}
