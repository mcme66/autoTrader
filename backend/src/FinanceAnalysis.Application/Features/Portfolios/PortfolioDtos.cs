namespace FinanceAnalysis.Application.Features.Portfolios;

/// <summary>A portfolio without its holdings, for list views.</summary>
public sealed record PortfolioDto(
    Guid Id,
    string Name,
    string? Description,
    string BaseCurrency,
    bool IsDefault,
    int HoldingCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <summary>
/// A valued position.
/// </summary>
/// <remarks>
/// Market-value fields are null when the symbol has no price history yet, so a freshly seeded
/// database renders as "awaiting prices" rather than as a portfolio worth zero.
/// </remarks>
public sealed record HoldingDto(
    Guid Id,
    string Symbol,
    string CompanyName,
    string? SectorKey,
    string? SectorName,
    decimal Quantity,
    decimal AverageCost,
    decimal CostBasis,
    decimal? LatestClose,
    DateOnly? PriceAsOf,
    decimal? MarketValue,
    decimal? UnrealizedGain,
    decimal? UnrealizedGainPercent,
    decimal? DayChange,
    decimal? DayChangePercent,
    decimal? Weight,
    DateOnly? OpenedOn,
    string? Notes);

/// <summary>Share of a portfolio's market value attributable to one sector.</summary>
public sealed record SectorAllocationDto(
    string SectorKey,
    string SectorName,
    decimal MarketValue,
    decimal Weight);

/// <summary>A portfolio with its holdings valued and aggregated.</summary>
public sealed record PortfolioSummaryDto(
    PortfolioDto Portfolio,
    decimal TotalCostBasis,
    decimal? TotalMarketValue,
    decimal? TotalUnrealizedGain,
    decimal? TotalUnrealizedGainPercent,
    decimal? DayChange,
    decimal? DayChangePercent,
    DateOnly? ValuedAsOf,
    IReadOnlyList<HoldingDto> Holdings,
    IReadOnlyList<SectorAllocationDto> SectorAllocation);

public sealed record CreatePortfolioRequest(
    string Name,
    string? Description,
    string? BaseCurrency,
    bool IsDefault);

public sealed record UpdatePortfolioRequest(string Name, string? Description, bool IsDefault);

public sealed record CreateHoldingRequest(
    string Symbol,
    decimal Quantity,
    decimal AverageCost,
    DateOnly? OpenedOn,
    string? Notes);

public sealed record UpdateHoldingRequest(
    decimal Quantity,
    decimal AverageCost,
    DateOnly? OpenedOn,
    string? Notes);
