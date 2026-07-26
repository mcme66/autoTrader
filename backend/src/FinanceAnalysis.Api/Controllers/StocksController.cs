using FinanceAnalysis.Application.Abstractions.Persistence.Queries;
using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Application.Features.Recommendations;
using FinanceAnalysis.Application.Features.Stocks;
using FinanceAnalysis.Domain.Identity;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAnalysis.Api.Controllers;

/// <summary>The stock catalogue and its price history.</summary>
[ApiController]
[Route("api/stocks")]
[Authorize]
[Produces("application/json")]
public sealed class StocksController(
    IStockService stocks,
    IRecommendationService recommendations) : ControllerBase
{
    /// <summary>Searches the catalogue by symbol or company name.</summary>
    /// <param name="query">Free-text match against symbol and company name.</param>
    /// <param name="sector">GICS sector key to filter by.</param>
    /// <param name="trackedOnly">Exclude symbols that are no longer collected.</param>
    /// <param name="sortBy">Sort field.</param>
    /// <param name="descending">Reverse the sort.</param>
    /// <param name="page">One-based page number.</param>
    /// <param name="pageSize">Results per page, capped server-side.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<PagedResult<StockDto>> SearchAsync(
        [FromQuery] string? query = null,
        [FromQuery] string? sector = null,
        [FromQuery] bool trackedOnly = true,
        [FromQuery] StockSortOrder sortBy = StockSortOrder.Symbol,
        [FromQuery] bool descending = false,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null,
        CancellationToken cancellationToken = default) =>
        stocks.SearchAsync(
            new StockSearchCriteria(new PageRequest(page, pageSize), query, sector, trackedOnly, sortBy, descending),
            cancellationToken);

    /// <summary>Returns one symbol with its descriptive data and summary statistics.</summary>
    [HttpGet("{symbol}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<StockDetailDto> GetAsync(string symbol, CancellationToken cancellationToken) =>
        stocks.GetBySymbolAsync(symbol, cancellationToken);

    /// <summary>Returns OHLCV history for a symbol, defaulting to the last year.</summary>
    [HttpGet("{symbol}/prices")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<PriceHistoryDto> GetPricesAsync(
        string symbol,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        CancellationToken cancellationToken = default) =>
        stocks.GetPriceHistoryAsync(symbol, from, to, cancellationToken);

    /// <summary>Returns the model predictions recorded for a symbol, newest first.</summary>
    [HttpGet("{symbol}/predictions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<IReadOnlyList<RecommendationDto>> GetPredictionsAsync(
        string symbol,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default) =>
        recommendations.GetForSymbolAsync(symbol, limit, cancellationToken);

    /// <summary>
    /// Starts or stops collecting a symbol. Restricted to administrators because it changes
    /// what the nightly job downloads.
    /// </summary>
    [HttpPatch("{symbol}/tracking")]
    [Authorize(Roles = RoleNames.Administrator)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<StockDto> SetTrackingAsync(
        string symbol,
        UpdateTrackingRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return stocks.SetTrackingAsync(symbol, request.IsTracked, cancellationToken);
    }
}
