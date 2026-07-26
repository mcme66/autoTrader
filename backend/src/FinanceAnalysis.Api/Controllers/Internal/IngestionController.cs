using FinanceAnalysis.Api.Security;
using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Application.Features.Ingestion;
using FinanceAnalysis.Domain.Enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAnalysis.Api.Controllers.Internal;

/// <summary>
/// Ingestion triggers for the server's scheduled task.
/// </summary>
/// <remarks>
/// Deliberately not a scheduler. An external cron job owns the timing, which keeps schedule
/// changes out of the deployment cycle and means a missed run can be re-triggered by hand. The
/// endpoints return <c>202 Accepted</c> with a run id; poll <c>GET runs/{id}</c> for the
/// outcome. Excluded from OpenAPI and gated by API key, network policy, and an nginx deny rule.
/// </remarks>
[ApiController]
[Route("api/internal/ingestion")]
[Authorize(Policy = InternalEndpointPolicy.Name)]
[ApiExplorerSettings(IgnoreApi = true)]
[Produces("application/json")]
public sealed class IngestionController(IIngestionCoordinator coordinator) : ControllerBase
{
    /// <summary>
    /// Queues collection of one trading day's prices. Omit the date for the most recent
    /// likely trading day, which is what the nightly cron entry does.
    /// </summary>
    [HttpPost("daily-prices")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IngestionRunDto>> TriggerDailyAsync(
        [FromQuery] DateOnly? tradeDate = null,
        CancellationToken cancellationToken = default)
    {
        var run = await coordinator.EnqueueDailyPricesAsync(tradeDate, cancellationToken);
        return AcceptedAtAction(nameof(GetRunAsync), new { runId = run.Id }, run);
    }

    /// <summary>Queues a historical load across an inclusive date range.</summary>
    [HttpPost("backfill")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IngestionRunDto>> TriggerBackfillAsync(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var run = await coordinator.EnqueueBackfillAsync(from, to, cancellationToken);
        return AcceptedAtAction(nameof(GetRunAsync), new { runId = run.Id }, run);
    }

    /// <summary>Returns recent ingestion attempts, newest first.</summary>
    [HttpGet("runs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<PagedResult<IngestionRunDto>> GetRunsAsync(
        [FromQuery] IngestionRunType? runType = null,
        [FromQuery] IngestionRunStatus? status = null,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null,
        CancellationToken cancellationToken = default) =>
        coordinator.GetRunsAsync(new PageRequest(page, pageSize), runType, status, cancellationToken);

    /// <summary>Returns one ingestion attempt.</summary>
    [HttpGet("runs/{runId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IngestionRunDto> GetRunAsync(Guid runId, CancellationToken cancellationToken) =>
        coordinator.GetRunAsync(runId, cancellationToken);
}
