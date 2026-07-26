using FinanceAnalysis.Domain.Portfolios;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceAnalysis.Infrastructure.Persistence.Configurations;

internal sealed class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.BaseCurrency).HasMaxLength(3).IsRequired();

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.Name }).IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(x => x.Portfolios)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Holdings).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class PortfolioHoldingConfiguration : IEntityTypeConfiguration<PortfolioHolding>
{
    public void Configure(EntityTypeBuilder<PortfolioHolding> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        // Fractional shares are common, hence six decimal places rather than an integer.
        builder.Property(x => x.Quantity).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.AverageCost).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasIndex(x => new { x.PortfolioId, x.StockId }).IsUnique();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_portfolio_holdings_quantity", "quantity > 0");
            t.HasCheckConstraint("ck_portfolio_holdings_average_cost", "average_cost >= 0");
        });

        builder.HasOne(x => x.Portfolio)
            .WithMany(x => x.Holdings)
            .HasForeignKey(x => x.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Stock)
            .WithMany()
            .HasForeignKey(x => x.StockId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.CostBasis);
    }
}
