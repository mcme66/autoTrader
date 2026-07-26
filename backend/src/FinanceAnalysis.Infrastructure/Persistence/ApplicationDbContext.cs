using System.Reflection;

using FinanceAnalysis.Domain.Catalog;
using FinanceAnalysis.Domain.Identity;
using FinanceAnalysis.Domain.MarketData;
using FinanceAnalysis.Domain.Portfolios;
using FinanceAnalysis.Domain.Predictions;

using Microsoft.EntityFrameworkCore;

namespace FinanceAnalysis.Infrastructure.Persistence;

/// <summary>
/// The single EF Core context for the application schema.
/// </summary>
/// <remarks>
/// Physical naming is snake_case (configured in <c>DependencyInjection</c>) so the tables read
/// naturally from psql and from the external Python ML pipeline, which uses the same database.
/// </remarks>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Sector> Sectors => Set<Sector>();

    public DbSet<Industry> Industries => Set<Industry>();

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<Stock> Stocks => Set<Stock>();

    public DbSet<DailyPrice> DailyPrices => Set<DailyPrice>();

    public DbSet<DataSource> DataSources => Set<DataSource>();

    public DbSet<IngestionRun> IngestionRuns => Set<IngestionRun>();

    public DbSet<Portfolio> Portfolios => Set<Portfolio>();

    public DbSet<PortfolioHolding> PortfolioHoldings => Set<PortfolioHolding>();

    // Written by MLPipeline_Jordan; mapped read-only here. See MlModelConfiguration.
    public DbSet<MlModel> MlModels => Set<MlModel>();

    public DbSet<MlPrediction> MlPredictions => Set<MlPrediction>();

    public DbSet<MlPredictionHistory> MlPredictionHistory => Set<MlPredictionHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
