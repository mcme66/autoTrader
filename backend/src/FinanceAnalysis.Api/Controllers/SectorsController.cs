using FinanceAnalysis.Application.Features.Sectors;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAnalysis.Api.Controllers;

/// <summary>The GICS sector reference list used by filters and charts.</summary>
[ApiController]
[Route("api/sectors")]
[Authorize]
[Produces("application/json")]
public sealed class SectorsController(ISectorService sectors) : ControllerBase
{
    /// <summary>Returns every sector in display order.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<IReadOnlyList<SectorDto>> GetAllAsync(CancellationToken cancellationToken) =>
        sectors.GetAllAsync(cancellationToken);
}
