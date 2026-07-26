using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

namespace FinanceAnalysis.IntegrationTests;

/// <summary>
/// Lightweight smoke test against the in-memory test host.
/// Full DB-backed coverage needs Docker (Testcontainers) and is intentionally out of scope here.
/// </summary>
public sealed class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task LiveHealthEndpoint_IsAnonymousAndResponds()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(
            new Uri("/health/live", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // With or without a live database the live endpoint should not 401/404.
        response.StatusCode.ShouldNotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.ShouldNotBe(HttpStatusCode.NotFound);
    }
}
