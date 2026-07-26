using FinanceAnalysis.Api.Security;
using FinanceAnalysis.Application.Features.Universe;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAnalysis.Api.Controllers.Internal;

/// <summary>Reconciles the declared universe file into the catalogue.</summary>
[ApiController]
[Route("api/internal/universe")]
[Authorize(Policy = InternalEndpointPolicy.Name)]
[ApiExplorerSettings(IgnoreApi = true)]
[Produces("application/json")]
public sealed class UniverseController(IUniverseSyncService universe) : ControllerBase
{
    /// <summary>
    /// Applies the universe file: adds new symbols, updates classifications, and marks removed
    /// symbols untracked. Runs synchronously because it is a few hundred rows and the operator
    /// wants the diff in the response.
    /// </summary>
    [HttpPost("sync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<UniverseSyncResult> SyncAsync(CancellationToken cancellationToken) =>
        universe.SyncAsync(cancellationToken);
}
