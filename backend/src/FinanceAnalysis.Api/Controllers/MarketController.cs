using FinanceAnalysis.Application.Features.MarketOverview;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAnalysis.Api.Controllers;

/// <summary>Whole-market aggregates for the overview page.</summary>
[ApiController]
[Route("api/market")]
[Authorize]
[Produces("application/json")]
public sealed class MarketController(IMarketOverviewService overview) : ControllerBase
{
    /// <summary>Breadth, sector performance and movers for the latest stored trading day.</summary>
    /// <param name="movers">How many gainers, losers and most-active names to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("overview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<MarketOverviewDto> GetOverviewAsync(
        [FromQuery] int movers = 5,
        CancellationToken cancellationToken = default) =>
        overview.GetAsync(movers, cancellationToken);
}
