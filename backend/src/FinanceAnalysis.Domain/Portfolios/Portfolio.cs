using FinanceAnalysis.Domain.Common;
using FinanceAnalysis.Domain.Exceptions;
using FinanceAnalysis.Domain.Identity;

namespace FinanceAnalysis.Domain.Portfolios;

/// <summary>
/// A named collection of holdings owned by a single user.
/// </summary>
public sealed class Portfolio : Entity<Guid>, IAuditable
{
    private readonly List<PortfolioHolding> _holdings = [];

    private Portfolio()
    {
    }

    public Portfolio(Guid userId, string name, string? description, string baseCurrency, bool isDefault)
    {
        Id = SequentialGuid.New();
        UserId = userId;
        Name = name.Trim();
        Description = description?.Trim();
        BaseCurrency = baseCurrency;
        IsDefault = isDefault;
    }

    public Guid UserId { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public string BaseCurrency { get; private set; } = null!;

    /// <summary>The portfolio surfaced on the dashboard when the user has several.</summary>
    public bool IsDefault { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public User User { get; private set; } = null!;

    public IReadOnlyCollection<PortfolioHolding> Holdings => _holdings;

    public void Rename(string name, string? description)
    {
        Name = name.Trim();
        Description = description?.Trim();
    }

    public void MarkAsDefault() => IsDefault = true;

    public void ClearDefault() => IsDefault = false;

    public PortfolioHolding AddHolding(int stockId, decimal quantity, decimal averageCost, DateOnly? openedOn, string? notes)
    {
        if (_holdings.Exists(h => h.StockId == stockId))
        {
            throw new ConflictException("That stock is already held in this portfolio.");
        }

        var holding = new PortfolioHolding(Id, stockId, quantity, averageCost, openedOn, notes);
        _holdings.Add(holding);
        return holding;
    }

    public PortfolioHolding GetHolding(Guid holdingId) =>
        _holdings.Find(h => h.Id == holdingId)
        ?? throw new NotFoundException("Holding", holdingId);

    public void RemoveHolding(Guid holdingId) => _holdings.Remove(GetHolding(holdingId));
}
