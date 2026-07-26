using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Application.Abstractions.Persistence.Projections;
using FinanceAnalysis.Domain.Catalog;
using FinanceAnalysis.Domain.Exceptions;
using FinanceAnalysis.Domain.Portfolios;

using Microsoft.Extensions.Logging;

namespace FinanceAnalysis.Application.Features.Portfolios;

public sealed class PortfolioService(
    IPortfolioRepository portfolios,
    IStockRepository stocks,
    IUnitOfWork unitOfWork,
    ILogger<PortfolioService> logger) : IPortfolioService
{
    private const string DefaultCurrency = "USD";
    private const int MaxPortfoliosPerUser = 25;
    private const int MaxHoldingsPerPortfolio = 500;

    public async Task<IReadOnlyList<PortfolioDto>> GetForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var owned = await portfolios.GetForUserAsync(userId, cancellationToken).ConfigureAwait(false);
        var results = new List<PortfolioDto>(owned.Count);

        foreach (var portfolio in owned)
        {
            var holdingCount = await portfolios
                .CountHoldingsAsync(portfolio.Id, cancellationToken)
                .ConfigureAwait(false);

            results.Add(ToDto(portfolio, holdingCount));
        }

        return results;
    }

    public async Task<PortfolioDto> GetAsync(
        Guid userId,
        Guid portfolioId,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await RequireOwnedAsync(userId, portfolioId, cancellationToken).ConfigureAwait(false);
        var holdingCount = await portfolios.CountHoldingsAsync(portfolioId, cancellationToken).ConfigureAwait(false);

        return ToDto(portfolio, holdingCount);
    }

    public async Task<PortfolioSummaryDto> GetSummaryAsync(
        Guid userId,
        Guid portfolioId,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await RequireOwnedAsync(userId, portfolioId, cancellationToken).ConfigureAwait(false);
        return await BuildSummaryAsync(portfolio, cancellationToken).ConfigureAwait(false);
    }

    public async Task<PortfolioSummaryDto?> GetDefaultSummaryAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await portfolios.FindDefaultForUserAsync(userId, cancellationToken).ConfigureAwait(false);

        if (portfolio is null)
        {
            // A user with portfolios but no default still deserves a dashboard.
            var owned = await portfolios.GetForUserAsync(userId, cancellationToken).ConfigureAwait(false);
            portfolio = owned.Count == 0 ? null : owned[0];
        }

        return portfolio is null
            ? null
            : await BuildSummaryAsync(portfolio, cancellationToken).ConfigureAwait(false);
    }

    public async Task<PortfolioDto> CreateAsync(
        Guid userId,
        CreatePortfolioRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existing = await portfolios.GetForUserAsync(userId, cancellationToken).ConfigureAwait(false);

        if (existing.Count >= MaxPortfoliosPerUser)
        {
            throw new BusinessRuleException($"A user may own at most {MaxPortfoliosPerUser} portfolios.");
        }

        var name = request.Name.Trim();

        if (await portfolios.NameExistsForUserAsync(userId, name, null, cancellationToken).ConfigureAwait(false))
        {
            throw new ConflictException($"You already have a portfolio named '{name}'.");
        }

        // The first portfolio is always the default, so the dashboard has something to show.
        var isDefault = request.IsDefault || existing.Count == 0;

        if (isDefault)
        {
            await ClearExistingDefaultAsync(userId, null, cancellationToken).ConfigureAwait(false);
        }

        var portfolio = new Portfolio(
            userId,
            name,
            request.Description,
            NormalizeCurrency(request.BaseCurrency),
            isDefault);

        portfolios.Add(portfolio);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("User {UserId} created portfolio {PortfolioId}.", userId, portfolio.Id);

        return ToDto(portfolio, 0);
    }

    public async Task<PortfolioDto> UpdateAsync(
        Guid userId,
        Guid portfolioId,
        UpdatePortfolioRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var portfolio = await RequireOwnedAsync(userId, portfolioId, cancellationToken).ConfigureAwait(false);
        var name = request.Name.Trim();

        if (await portfolios
                .NameExistsForUserAsync(userId, name, portfolioId, cancellationToken)
                .ConfigureAwait(false))
        {
            throw new ConflictException($"You already have a portfolio named '{name}'.");
        }

        portfolio.Rename(name, request.Description);

        if (request.IsDefault && !portfolio.IsDefault)
        {
            await ClearExistingDefaultAsync(userId, portfolioId, cancellationToken).ConfigureAwait(false);
            portfolio.MarkAsDefault();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var holdingCount = await portfolios.CountHoldingsAsync(portfolioId, cancellationToken).ConfigureAwait(false);
        return ToDto(portfolio, holdingCount);
    }

    public async Task DeleteAsync(Guid userId, Guid portfolioId, CancellationToken cancellationToken = default)
    {
        var portfolio = await RequireOwnedAsync(userId, portfolioId, cancellationToken).ConfigureAwait(false);

        portfolios.Remove(portfolio);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Keep exactly one default so the dashboard never has to guess.
        if (portfolio.IsDefault)
        {
            var remaining = await portfolios.GetForUserAsync(userId, cancellationToken).ConfigureAwait(false);

            if (remaining.Count > 0)
            {
                remaining[0].MarkAsDefault();
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        logger.LogInformation("User {UserId} deleted portfolio {PortfolioId}.", userId, portfolioId);
    }

    public async Task<HoldingDto> AddHoldingAsync(
        Guid userId,
        Guid portfolioId,
        CreateHoldingRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var portfolio = await RequireOwnedWithHoldingsAsync(userId, portfolioId, cancellationToken)
            .ConfigureAwait(false);

        if (portfolio.Holdings.Count >= MaxHoldingsPerPortfolio)
        {
            throw new BusinessRuleException(
                $"A portfolio may contain at most {MaxHoldingsPerPortfolio} holdings.");
        }

        var symbol = Stock.NormalizeSymbol(request.Symbol);
        var stock = await stocks.FindBySymbolAsync(symbol, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException("Stock", symbol);

        var holding = portfolio.AddHolding(
            stock.Id,
            request.Quantity,
            request.AverageCost,
            request.OpenedOn,
            request.Notes);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await GetHoldingDtoAsync(portfolioId, holding.Id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<HoldingDto> UpdateHoldingAsync(
        Guid userId,
        Guid portfolioId,
        Guid holdingId,
        UpdateHoldingRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var portfolio = await RequireOwnedWithHoldingsAsync(userId, portfolioId, cancellationToken)
            .ConfigureAwait(false);

        portfolio
            .GetHolding(holdingId)
            .Update(request.Quantity, request.AverageCost, request.OpenedOn, request.Notes);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await GetHoldingDtoAsync(portfolioId, holdingId, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveHoldingAsync(
        Guid userId,
        Guid portfolioId,
        Guid holdingId,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await RequireOwnedWithHoldingsAsync(userId, portfolioId, cancellationToken)
            .ConfigureAwait(false);

        portfolio.RemoveHolding(holdingId);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<PortfolioSummaryDto> BuildSummaryAsync(
        Portfolio portfolio,
        CancellationToken cancellationToken)
    {
        var valuations = await portfolios
            .GetHoldingValuationsAsync(portfolio.Id, cancellationToken)
            .ConfigureAwait(false);

        var totalCost = valuations.Sum(v => v.Quantity * v.AverageCost);

        // Only holdings with a price contribute to market value. Mixing priced and unpriced
        // positions into one number would silently understate the portfolio.
        var priced = valuations.Where(v => v.LatestClose is not null).ToList();
        decimal? totalValue = priced.Count == 0 ? null : priced.Sum(v => v.Quantity * v.LatestClose!.Value);

        var holdings = new List<HoldingDto>(valuations.Count);
        foreach (var valuation in valuations)
        {
            holdings.Add(ToHoldingDto(valuation, totalValue));
        }

        decimal? dayChange = priced.Count == 0
            ? null
            : priced
                .Where(v => v.PreviousClose is not null)
                .Sum(v => v.Quantity * (v.LatestClose!.Value - v.PreviousClose!.Value));

        var previousValue = priced
            .Where(v => v.PreviousClose is not null)
            .Sum(v => v.Quantity * v.PreviousClose!.Value);

        decimal? totalGain = totalValue is null ? null : totalValue - totalCost;

        return new PortfolioSummaryDto(
            ToDto(portfolio, valuations.Count),
            Round(totalCost),
            Round(totalValue),
            Round(totalGain),
            totalGain is null || totalCost == 0m ? null : Round(totalGain.Value / totalCost * 100m),
            Round(dayChange),
            dayChange is null || previousValue == 0m ? null : Round(dayChange.Value / previousValue * 100m),
            valuations.Count == 0 ? null : valuations.Max(v => v.LatestTradeDate),
            holdings,
            BuildSectorAllocation(valuations, totalValue));
    }

    private static IReadOnlyList<SectorAllocationDto> BuildSectorAllocation(
        IReadOnlyList<HoldingValuation> valuations,
        decimal? totalValue)
    {
        if (totalValue is null or 0m)
        {
            return [];
        }

        return
        [
            .. valuations
                .Where(v => v.LatestClose is not null)
                .GroupBy(v => (Key: v.SectorKey ?? "Unclassified", Name: v.SectorName ?? "Unclassified"))
                .Select(g =>
                {
                    var value = g.Sum(v => v.Quantity * v.LatestClose!.Value);
                    return new SectorAllocationDto(
                        g.Key.Key,
                        g.Key.Name,
                        Round(value),
                        Round(value / totalValue.Value * 100m));
                })
                .OrderByDescending(a => a.MarketValue),
        ];
    }

    private static HoldingDto ToHoldingDto(HoldingValuation v, decimal? portfolioValue)
    {
        var costBasis = v.Quantity * v.AverageCost;
        decimal? marketValue = v.LatestClose is null ? null : v.Quantity * v.LatestClose.Value;
        decimal? gain = marketValue is null ? null : marketValue - costBasis;

        decimal? dayChange = v.LatestClose is null || v.PreviousClose is null
            ? null
            : v.Quantity * (v.LatestClose.Value - v.PreviousClose.Value);

        return new HoldingDto(
            v.HoldingId,
            v.Symbol,
            v.CompanyName,
            v.SectorKey,
            v.SectorName,
            v.Quantity,
            v.AverageCost,
            Round(costBasis),
            v.LatestClose,
            v.LatestTradeDate,
            Round(marketValue),
            Round(gain),
            gain is null || costBasis == 0m ? null : Round(gain.Value / costBasis * 100m),
            Round(dayChange),
            dayChange is null || v.PreviousClose is null || v.PreviousClose == 0m
                ? null
                : Round((v.LatestClose!.Value - v.PreviousClose.Value) / v.PreviousClose.Value * 100m),
            marketValue is null || portfolioValue is null or 0m
                ? null
                : Round(marketValue.Value / portfolioValue.Value * 100m),
            v.OpenedOn,
            v.Notes);
    }

    private async Task<HoldingDto> GetHoldingDtoAsync(
        Guid portfolioId,
        Guid holdingId,
        CancellationToken cancellationToken)
    {
        var valuations = await portfolios
            .GetHoldingValuationsAsync(portfolioId, cancellationToken)
            .ConfigureAwait(false);

        var priced = valuations.Where(v => v.LatestClose is not null).ToList();
        decimal? totalValue = priced.Count == 0 ? null : priced.Sum(v => v.Quantity * v.LatestClose!.Value);

        var valuation = valuations.FirstOrDefault(v => v.HoldingId == holdingId)
            ?? throw new NotFoundException("Holding", holdingId);

        return ToHoldingDto(valuation, totalValue);
    }

    private async Task<Portfolio> RequireOwnedAsync(
        Guid userId,
        Guid portfolioId,
        CancellationToken cancellationToken)
    {
        var portfolio = await portfolios.FindByIdAsync(portfolioId, cancellationToken).ConfigureAwait(false);
        return Authorize(portfolio, userId, portfolioId);
    }

    private async Task<Portfolio> RequireOwnedWithHoldingsAsync(
        Guid userId,
        Guid portfolioId,
        CancellationToken cancellationToken)
    {
        var portfolio = await portfolios
            .FindByIdWithHoldingsAsync(portfolioId, cancellationToken)
            .ConfigureAwait(false);

        return Authorize(portfolio, userId, portfolioId);
    }

    /// <summary>
    /// Reports another user's portfolio as missing rather than forbidden, so the API cannot be
    /// used to probe which portfolio ids exist.
    /// </summary>
    private static Portfolio Authorize(Portfolio? portfolio, Guid userId, Guid portfolioId) =>
        portfolio is null || portfolio.UserId != userId
            ? throw new NotFoundException("Portfolio", portfolioId)
            : portfolio;

    private async Task ClearExistingDefaultAsync(
        Guid userId,
        Guid? excluding,
        CancellationToken cancellationToken)
    {
        var current = await portfolios.FindDefaultForUserAsync(userId, cancellationToken).ConfigureAwait(false);

        if (current is not null && current.Id != excluding)
        {
            current.ClearDefault();
        }
    }

    private static string NormalizeCurrency(string? currency) =>
        string.IsNullOrWhiteSpace(currency) ? DefaultCurrency : currency.Trim().ToUpperInvariant();

    private static PortfolioDto ToDto(Portfolio portfolio, int holdingCount) => new(
        portfolio.Id,
        portfolio.Name,
        portfolio.Description,
        portfolio.BaseCurrency,
        portfolio.IsDefault,
        holdingCount,
        portfolio.CreatedAt,
        portfolio.UpdatedAt);

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static decimal? Round(decimal? value) => value is null ? null : Round(value.Value);
}
