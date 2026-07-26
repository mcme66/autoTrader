using System.Text;

using FinanceAnalysis.Api.Configuration;
using FinanceAnalysis.Api.Diagnostics;
using FinanceAnalysis.Api.Extensions;
using FinanceAnalysis.Api.Filters;
using FinanceAnalysis.Api.Security;
using FinanceAnalysis.Application;
using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Application.Configuration;
using FinanceAnalysis.Infrastructure;
using FinanceAnalysis.Infrastructure.Persistence;

using FluentValidation;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using Scalar.AspNetCore;

using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services
        .AddOptions<SecurityOptions>()
        .Bind(builder.Configuration.GetSection(SecurityOptions.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUser, CurrentUser>();
    builder.Services.AddSingleton<IAuthorizationHandler, InternalNetworkAuthorizationHandler>();

    var authOptions = builder.Configuration
        .GetSection(AuthenticationOptions.SectionName)
        .Get<AuthenticationOptions>()
        ?? new AuthenticationOptions();

    var signingKey = authOptions.Jwt.SigningKey
        ?? throw new InvalidOperationException(
            "Auth:Jwt:SigningKey is not configured. Set Auth__Jwt__SigningKey or use user-secrets.");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = authOptions.Jwt.Issuer,
                ValidAudience = authOptions.Jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                ClockSkew = TimeSpan.FromMinutes(1),
                NameClaimType = "sub",
                RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            };
        })
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, InternalApiKeyAuthenticationHandler>(
            InternalApiKeyDefaults.AuthenticationScheme,
            _ => { });

    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            .Build();

        options.AddPolicy(InternalEndpointPolicy.Name, policy =>
        {
            policy.AddAuthenticationSchemes(InternalApiKeyDefaults.AuthenticationScheme);
            policy.RequireAuthenticatedUser();
            policy.RequireRole(InternalApiKeyDefaults.Role);
            policy.AddRequirements(new InternalNetworkRequirement());
        });
    });

    var securityOptions = builder.Configuration
        .GetSection(SecurityOptions.SectionName)
        .Get<SecurityOptions>()
        ?? new SecurityOptions();

    if (securityOptions.CorsOrigins.Count > 0)
    {
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy
                    .WithOrigins([.. securityOptions.CorsOrigins])
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });
    }

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidationActionFilter>();
    });

    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    builder.Services
        .AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>("database");

    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "Finance Analysis Platform API",
                Version = "v1",
                Description = "Collect, store, and expose equity market data and ML predictions.",
            };
            return Task.CompletedTask;
        });
    });

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();

    if (securityOptions.CorsOrigins.Count > 0)
    {
        app.UseCors();
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health/live").AllowAnonymous();
    app.MapHealthChecks("/health/ready").AllowAnonymous();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi().AllowAnonymous();
        app.MapScalarApiReference().AllowAnonymous();
    }

    await app.InitializeAsync();

    if (args.Any(a => string.Equals(a, "--migrate", StringComparison.OrdinalIgnoreCase)))
    {
        Log.Information("Database migrations applied; exiting (--migrate).");
        return;
    }

    if (args.Any(a => string.Equals(a, "--seed", StringComparison.OrdinalIgnoreCase)))
    {
        await DevelopmentSeeder.SeedAsync(app.Services);
        return;
    }

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Finance Analysis Platform API terminated unexpectedly.");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

/// <summary>Marker for WebApplicationFactory in integration tests.</summary>
public partial class Program;
