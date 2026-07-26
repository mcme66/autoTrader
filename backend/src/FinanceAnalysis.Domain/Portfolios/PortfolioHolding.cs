using FinanceAnalysis.Domain.Catalog;
using FinanceAnalysis.Domain.Common;
using FinanceAnalysis.Domain.Exceptions;

namespace FinanceAnalysis.Domain.Portfolios;

/// <summary>
/// A position in one symbol within a <see cref="Portfolio"/>.
/// </summary>
/// <remarks>
/// The position is stored as an aggregate quantity and average cost rather than as a ledger
/// of buys and sells. A per-transaction ledger is a plausible future addition; it would be a
/// new table referencing this one, not a change to this shape.
/// </remarks>
public sealed class PortfolioHolding : Entity<Guid>, IAuditable
{
    private PortfolioHolding()
    {
    }

    internal PortfolioHolding(
        Guid portfolioId,
        int stockId,
        decimal quantity,
        decimal averageCost,
        DateOnly? openedOn,
        string? notes)
    {
        Id = SequentialGuid.New();
        PortfolioId = portfolioId;
        StockId = stockId;
        SetPosition(quantity, averageCost);
        OpenedOn = openedOn;
        Notes = notes?.Trim();
    }

    public Guid PortfolioId { get; private set; }

    public int StockId { get; private set; }

    public decimal Quantity { get; private set; }

    /// <summary>Average price paid per share, in the portfolio's base currency.</summary>
    public decimal AverageCost { get; private set; }

    public DateOnly? OpenedOn { get; private set; }

    public string? Notes { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Portfolio Portfolio { get; private set; } = null!;

    public Stock Stock { get; private set; } = null!;

    public decimal CostBasis => Quantity * AverageCost;

    public void Update(decimal quantity, decimal averageCost, DateOnly? openedOn, string? notes)
    {
        SetPosition(quantity, averageCost);
        OpenedOn = openedOn;
        Notes = notes?.Trim();
    }

    private void SetPosition(decimal quantity, decimal averageCost)
    {
        if (quantity <= 0)
        {
            throw new BusinessRuleException("Holding quantity must be greater than zero.");
        }

        if (averageCost < 0)
        {
            throw new BusinessRuleException("Average cost cannot be negative.");
        }

        Quantity = quantity;
        AverageCost = averageCost;
    }
}
