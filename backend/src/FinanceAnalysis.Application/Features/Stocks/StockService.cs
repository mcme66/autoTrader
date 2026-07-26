using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Application.Abstractions.Persistence.Queries;
using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Domain.Catalog;
using FinanceAnalysis.Domain.Exceptions;

using Microsoft.Extensions.Logging;

namespace FinanceAnalysis.Application.Features.Stocks;

public sealed class StockService(
    IStockRepository stocks,
    ICompanyRepository companies,
    IDailyPriceRepository prices,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<StockService> logger) : IStockService
{
    /// <summary>
    /// Widest window a single history request may span. Twenty years covers any realistic chart
    /// while keeping one request from scanning the whole table.
    /// </summary>
    private const int MaxHistoryDays = 366 * 20;

    private const int DefaultHistoryDays = 365;

    public async Task<PagedResult<StockDto>> SearchAsync(
        StockSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var page = await stocks.SearchAsync(criteria, cancellationToken).ConfigureAwait(false);
        return page.Map(StockMappings.ToDto);
    }

    public async Task<StockDetailDto> GetBySymbolAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        var normalized = Stock.NormalizeSymbol(symbol);

        var summary = await stocks.GetSummaryBySymbolAsync(normalized, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException("Stock", normalized);

        var stock = await stocks.FindBySymbolAsync(normalized, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException("Stock", normalized);

        var company = await companies.FindByIdAsync(stock.CompanyId, cancellationToken).ConfigureAwait(false);

        var window = await prices
            .GetRangeAsync(
                stock.Id,
                clock.UtcToday.AddDays(-DefaultHistoryDays),
                clock.UtcToday,
                cancellationToken)
            .ConfigureAwait(false);

        return new StockDetailDto(
            summary.ToDto(),
            company?.Description,
            company?.HomepageUrl,
            company?.CountryCode,
            company?.EmployeeCount,
            company?.ListedOn,
            stock.DelistedOn,
            window.ToStatistics());
    }

    public async Task<PriceHistoryDto> GetPriceHistoryAsync(
        string symbol,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken = default)
    {
        var normalized = Stock.NormalizeSymbol(symbol);

        var stock = await stocks.FindBySymbolAsync(normalized, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException("Stock", normalized);

        var (rangeStart, rangeEnd) = ResolveWindow(fromDate, toDate);

        var bars = await prices
            .GetRangeAsync(stock.Id, rangeStart, rangeEnd, cancellationToken)
            .ConfigureAwait(false);

        return new PriceHistoryDto(
            normalized,
            rangeStart,
            rangeEnd,
            [.. bars.Select(StockMappings.ToDto)],
            bars.ToStatistics());
    }

    public async Task<StockDto> SetTrackingAsync(
        string symbol,
        bool isTracked,
        CancellationToken cancellationToken = default)
    {
        var normalized = Stock.NormalizeSymbol(symbol);

        var stock = await stocks.FindBySymbolAsync(normalized, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException("Stock", normalized);

        if (isTracked)
        {
            stock.StartTracking();
        }
        else
        {
            stock.StopTracking();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation(
            "Symbol {Symbol} is now {State}.",
            normalized,
            isTracked ? "tracked" : "untracked");

        var summary = await stocks.GetSummaryBySymbolAsync(normalized, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException("Stock", normalized);

        return summary.ToDto();
    }

    private (DateOnly Start, DateOnly End) ResolveWindow(DateOnly? fromDate, DateOnly? toDate)
    {
        var end = toDate ?? clock.UtcToday;
        var start = fromDate ?? end.AddDays(-DefaultHistoryDays);

        if (start > end)
        {
            throw new BusinessRuleException("'from' must not be later than 'to'.");
        }

        if (end.DayNumber - start.DayNumber > MaxHistoryDays)
        {
            throw new BusinessRuleException(
                $"The requested window spans more than {MaxHistoryDays} days. Narrow the range.");
        }

        return (start, end);
    }
}
