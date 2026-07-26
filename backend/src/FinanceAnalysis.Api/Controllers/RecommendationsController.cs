using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Application.Features.Recommendations;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAnalysis.Api.Controllers;

/// <summary>
/// Predictions written by the external ML pipeline.
/// </summary>
/// <remarks>
/// Read-only. This service never writes to the ML tables; an empty response means the pipeline
/// has not produced anything yet, which the client renders as a placeholder.
/// </remarks>
[ApiController]
[Route("api/recommendations")]
[Authorize]
[Produces("application/json")]
public sealed class RecommendationsController(IRecommendationService recommendations) : ControllerBase
{
    /// <summary>Returns the newest prediction per symbol, with model metadata and accuracy.</summary>
    /// <param name="model">Restrict to one model key.</param>
    /// <param name="page">One-based page number.</param>
    /// <param name="pageSize">Results per page, capped server-side.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<RecommendationsDto> GetAsync(
        [FromQuery] string? model = null,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null,
        CancellationToken cancellationToken = default) =>
        recommendations.GetLatestAsync(new PageRequest(page, pageSize), model, cancellationToken);
}
