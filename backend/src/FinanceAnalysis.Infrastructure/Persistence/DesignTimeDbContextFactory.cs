using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinanceAnalysis.Infrastructure.Persistence;

/// <summary>
/// Used only by <c>dotnet ef</c> at design time. The connection string is read from
/// <c>FINANCEANALYSIS_MIGRATIONS_CONNECTION</c> when present so migrations can be generated
/// without a running application, and never needs to be committed.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    private const string DefaultConnection =
        "Host=localhost;Port=5433;Database=finance_analysis;Username=finance;Password=finance";

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("FINANCEANALYSIS_MIGRATIONS_CONNECTION")
            ?? DefaultConnection;

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new ApplicationDbContext(options);
    }
}
