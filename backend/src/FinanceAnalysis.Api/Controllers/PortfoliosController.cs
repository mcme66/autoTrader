using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Application.Features.Portfolios;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAnalysis.Api.Controllers;

/// <summary>
/// Portfolios and their holdings.
/// </summary>
/// <remarks>
/// The caller's id comes from the token and is passed to the service on every call; the route
/// never determines whose data is touched. That keeps ownership enforcement in one place
/// instead of relying on each action to remember it.
/// </remarks>
[ApiController]
[Route("api/portfolios")]
[Authorize]
[Produces("application/json")]
public sealed class PortfoliosController(IPortfolioService portfolios, ICurrentUser currentUser) : ControllerBase
{
    /// <summary>Lists the caller's portfolios.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<IReadOnlyList<PortfolioDto>> GetAllAsync(CancellationToken cancellationToken) =>
        portfolios.GetForUserAsync(currentUser.RequireUserId(), cancellationToken);

    /// <summary>
    /// Returns the caller's default portfolio fully valued, or 204 when they have none yet.
    /// </summary>
    [HttpGet("default/summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<PortfolioSummaryDto>> GetDefaultSummaryAsync(CancellationToken cancellationToken)
    {
        var summary = await portfolios.GetDefaultSummaryAsync(currentUser.RequireUserId(), cancellationToken);
        return summary is null ? NoContent() : Ok(summary);
    }

    /// <summary>Returns one portfolio.</summary>
    [HttpGet("{portfolioId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<PortfolioDto> GetAsync(Guid portfolioId, CancellationToken cancellationToken) =>
        portfolios.GetAsync(currentUser.RequireUserId(), portfolioId, cancellationToken);

    /// <summary>Returns one portfolio with its holdings valued at the latest close.</summary>
    [HttpGet("{portfolioId:guid}/summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<PortfolioSummaryDto> GetSummaryAsync(Guid portfolioId, CancellationToken cancellationToken) =>
        portfolios.GetSummaryAsync(currentUser.RequireUserId(), portfolioId, cancellationToken);

    /// <summary>Creates a portfolio.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PortfolioDto>> CreateAsync(
        CreatePortfolioRequest request,
        CancellationToken cancellationToken)
    {
        var created = await portfolios.CreateAsync(currentUser.RequireUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetAsync), new { portfolioId = created.Id }, created);
    }

    /// <summary>Renames a portfolio or changes its default flag.</summary>
    [HttpPut("{portfolioId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<PortfolioDto> UpdateAsync(
        Guid portfolioId,
        UpdatePortfolioRequest request,
        CancellationToken cancellationToken) =>
        portfolios.UpdateAsync(currentUser.RequireUserId(), portfolioId, request, cancellationToken);

    /// <summary>Deletes a portfolio and its holdings.</summary>
    [HttpDelete("{portfolioId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid portfolioId, CancellationToken cancellationToken)
    {
        await portfolios.DeleteAsync(currentUser.RequireUserId(), portfolioId, cancellationToken);
        return NoContent();
    }

    /// <summary>Adds a position.</summary>
    [HttpPost("{portfolioId:guid}/holdings")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HoldingDto>> AddHoldingAsync(
        Guid portfolioId,
        CreateHoldingRequest request,
        CancellationToken cancellationToken)
    {
        var created = await portfolios.AddHoldingAsync(
            currentUser.RequireUserId(),
            portfolioId,
            request,
            cancellationToken);

        return CreatedAtAction(nameof(GetSummaryAsync), new { portfolioId }, created);
    }

    /// <summary>Updates a position's quantity, cost or notes.</summary>
    [HttpPut("{portfolioId:guid}/holdings/{holdingId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<HoldingDto> UpdateHoldingAsync(
        Guid portfolioId,
        Guid holdingId,
        UpdateHoldingRequest request,
        CancellationToken cancellationToken) =>
        portfolios.UpdateHoldingAsync(currentUser.RequireUserId(), portfolioId, holdingId, request, cancellationToken);

    /// <summary>Removes a position.</summary>
    [HttpDelete("{portfolioId:guid}/holdings/{holdingId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveHoldingAsync(
        Guid portfolioId,
        Guid holdingId,
        CancellationToken cancellationToken)
    {
        await portfolios.RemoveHoldingAsync(currentUser.RequireUserId(), portfolioId, holdingId, cancellationToken);
        return NoContent();
    }
}
