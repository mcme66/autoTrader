using FinanceAnalysis.Application.Abstractions.Security;
using FinanceAnalysis.Application.Features.Authentication;
using FinanceAnalysis.Application.Features.Ingestion;
using FinanceAnalysis.Application.Features.MarketOverview;
using FinanceAnalysis.Application.Features.Portfolios;
using FinanceAnalysis.Application.Features.Recommendations;
using FinanceAnalysis.Application.Features.Sectors;
using FinanceAnalysis.Application.Features.Stocks;
using FinanceAnalysis.Application.Features.Universe;
using FinanceAnalysis.Application.Features.Users;

using Microsoft.Extensions.DependencyInjection;

namespace FinanceAnalysis.Application;

/// <summary>
/// Registers the business-logic layer.
/// </summary>
/// <remarks>
/// Every service here is scoped because they all collaborate through the scoped unit of work,
/// and each depends only on abstractions that <c>Infrastructure</c> supplies. Nothing in this
/// method references EF Core, HTTP, or any vendor.
/// </remarks>
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registered by concrete type first: AuthenticationService is the sole implementation of
        // both seams, and a single scoped instance must back them so an OAuth callback and a
        // password login share the same unit of work.
        services.AddScoped<AuthenticationService>();
        services.AddScoped<IAuthenticationService>(sp => sp.GetRequiredService<AuthenticationService>());
        services.AddScoped<IIdentityLinker>(sp => sp.GetRequiredService<AuthenticationService>());

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<ISectorService, SectorService>();
        services.AddScoped<IMarketOverviewService, MarketOverviewService>();
        services.AddScoped<IPortfolioService, PortfolioService>();
        services.AddScoped<IRecommendationService, RecommendationService>();
        services.AddScoped<IUniverseSyncService, UniverseSyncService>();
        services.AddScoped<IIngestionCoordinator, IngestionCoordinator>();
        services.AddScoped<IIngestionExecutor, IngestionExecutor>();

        return services;
    }
}
