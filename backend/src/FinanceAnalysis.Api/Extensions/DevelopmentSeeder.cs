using FinanceAnalysis.Application.Abstractions.Ingestion;
using FinanceAnalysis.Application.Abstractions.MarketData;
using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Application.Abstractions.Security;
using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Application.Features.Ingestion;
using FinanceAnalysis.Application.Features.Universe;
using FinanceAnalysis.Domain.Enums;
using FinanceAnalysis.Domain.Identity;
using FinanceAnalysis.Domain.MarketData;
using FinanceAnalysis.Domain.Portfolios;

namespace FinanceAnalysis.Api.Extensions;

/// <summary>
/// Loads a known-good local dataset for manual testing. Invoked by <c>npm run seed</c> via
/// <c>dotnet run -- --seed</c>; never registered in the DI container of a production host.
/// </summary>
internal static class DevelopmentSeeder
{
    public const string DemoEmail = "demo@finance.local";
    public const string DemoPassword = "DemoPassword1!";
    public const string DemoDisplayName = "Demo Trader";

    private static readonly (string Symbol, decimal Quantity, decimal AverageCost)[] DemoHoldings =
    [
        ("AAPL", 25m, 178.50m),
        ("MSFT", 15m, 390.00m),
        ("NVDA", 10m, 120.00m),
        ("GOOGL", 12m, 165.25m),
        ("JPM", 20m, 195.00m),
    ];

    private const int BackfillCalendarDays = 45;

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        await using var scope = services.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DevelopmentSeeder");
        var environment = sp.GetRequiredService<IHostEnvironment>();

        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "Development seeding is only allowed when ASPNETCORE_ENVIRONMENT=Development.");
        }

        logger.LogInformation("Starting development seed.");

        var universe = await sp.GetRequiredService<IUniverseSyncService>()
            .SyncAsync(cancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation(
            "Universe sync: {Total} symbols in file, {Added} added, {Updated} updated, {Untracked} untracked.",
            universe.SymbolsInFile,
            universe.Added,
            universe.Updated,
            universe.Untracked);

        var user = await EnsureDemoUserAsync(sp, logger, cancellationToken).ConfigureAwait(false);
        await EnsureDemoPortfolioAsync(sp, user, logger, cancellationToken).ConfigureAwait(false);
        await EnsureSamplePricesAsync(sp, logger, cancellationToken).ConfigureAwait(false);

        logger.LogInformation(
            "Development seed finished. Sign in with {Email} / {Password}",
            DemoEmail,
            DemoPassword);
    }

    private static async Task<User> EnsureDemoUserAsync(
        IServiceProvider sp,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var users = sp.GetRequiredService<IUserRepository>();
        var existing = await users.FindByEmailWithRolesAsync(DemoEmail, cancellationToken).ConfigureAwait(false);

        if (existing is not null)
        {
            logger.LogInformation("Demo user {Email} already exists ({UserId}).", DemoEmail, existing.Id);
            return existing;
        }

        var roles = sp.GetRequiredService<IRoleRepository>();
        var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
        var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
        var clock = sp.GetRequiredService<IClock>();

        var adminRole = await roles.FindByNameAsync(RoleNames.Administrator, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException(
                "The Administrator role is missing. Apply migrations before seeding.");

        var user = User.CreateLocal(DemoEmail, DemoDisplayName, passwordHasher.Hash(DemoPassword));
        user.AssignRole(adminRole);
        user.RecordLogin(clock.UtcNow);
        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Created demo user {Email} ({UserId}).", DemoEmail, user.Id);
        return user;
    }

    private static async Task EnsureDemoPortfolioAsync(
        IServiceProvider sp,
        User user,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var portfolios = sp.GetRequiredService<IPortfolioRepository>();
        var stocks = sp.GetRequiredService<IStockRepository>();
        var unitOfWork = sp.GetRequiredService<IUnitOfWork>();

        var existing = await portfolios.FindDefaultForUserAsync(user.Id, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            logger.LogInformation(
                "Demo portfolio already exists for {Email} ({PortfolioId}).",
                DemoEmail,
                existing.Id);
            return;
        }

        var portfolio = new Portfolio(
            user.Id,
            name: "Demo Portfolio",
            description: "Seeded holdings for local testing.",
            baseCurrency: "USD",
            isDefault: true);

        var openedOn = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(-3));
        var added = 0;

        foreach (var (symbol, quantity, averageCost) in DemoHoldings)
        {
            var stock = await stocks.FindBySymbolAsync(symbol, cancellationToken).ConfigureAwait(false);
            if (stock is null)
            {
                logger.LogWarning(
                    "Skipping holding {Symbol}: not present after universe sync.",
                    symbol);
                continue;
            }

            portfolio.AddHolding(stock.Id, quantity, averageCost, openedOn, notes: "Seeded");
            added++;
        }

        portfolios.Add(portfolio);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation(
            "Created demo portfolio {PortfolioId} with {HoldingCount} holding(s).",
            portfolio.Id,
            added);
    }

    private static async Task EnsureSamplePricesAsync(
        IServiceProvider sp,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var prices = sp.GetRequiredService<IDailyPriceRepository>();
        var latest = await prices.GetLatestTradeDateAsync(cancellationToken).ConfigureAwait(false);
        if (latest is not null)
        {
            logger.LogInformation(
                "Daily prices already present (latest trade date {Latest}); skipping sample ingest.",
                latest);
            return;
        }

        var providers = sp.GetRequiredService<IMarketDataProviderResolver>();
        var dataSources = sp.GetRequiredService<IDataSourceRepository>();
        var runs = sp.GetRequiredService<IIngestionRunRepository>();
        var executor = sp.GetRequiredService<IIngestionExecutor>();
        var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
        var clock = sp.GetRequiredService<IClock>();

        var provider = providers.Resolve();
        var dataSource = await dataSources
            .GetOrCreateAsync(provider.Key, provider.DisplayName, cancellationToken)
            .ConfigureAwait(false);

        var to = clock.UtcToday.AddDays(-1);
        var from = to.AddDays(-(BackfillCalendarDays - 1));

        var run = IngestionRun.ForRange(dataSource.Id, from, to);
        runs.Add(run);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation(
            "Ingesting mock prices from {From} to {To} via provider {Provider}.",
            from,
            to,
            provider.Key);

        var job = new IngestionJob(
            run.Id,
            IngestionRunType.HistoricalBackfill,
            provider.Key,
            dataSource.Id,
            RangeStart: from,
            RangeEnd: to);

        await executor.ExecuteAsync(job, cancellationToken).ConfigureAwait(false);

        var finished = await runs.FindByIdAsync(run.Id, cancellationToken).ConfigureAwait(false);
        logger.LogInformation(
            "Sample price ingest finished with status {Status} ({Inserted} rows inserted).",
            finished?.Status,
            finished?.RecordsInserted ?? 0);
    }
}
