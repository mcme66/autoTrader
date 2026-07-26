using FinanceAnalysis.Domain.Catalog;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceAnalysis.Infrastructure.Persistence.Configurations;

internal sealed class SectorConfiguration : IEntityTypeConfiguration<Sector>
{
    public void Configure(EntityTypeBuilder<Sector> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();

        builder.HasIndex(x => x.Key).IsUnique();

        builder.Navigation(x => x.Industries).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Companies).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasData(SectorSeed.Rows);
    }
}

internal sealed class IndustryConfiguration : IEntityTypeConfiguration<Industry>
{
    public void Configure(EntityTypeBuilder<Industry> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(160).IsRequired();

        builder.HasIndex(x => new { x.SectorId, x.Name }).IsUnique();

        builder.HasOne(x => x.Sector)
            .WithMany(x => x.Industries)
            .HasForeignKey(x => x.SectorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.Companies).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Cik).HasMaxLength(16);
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.HomepageUrl).HasMaxLength(512);
        builder.Property(x => x.CountryCode).HasMaxLength(2);

        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.SectorId);

        builder.HasOne(x => x.Sector)
            .WithMany(x => x.Companies)
            .HasForeignKey(x => x.SectorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Industry)
            .WithMany(x => x.Companies)
            .HasForeignKey(x => x.IndustryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Navigation(x => x.Stocks).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Symbol).HasMaxLength(16).IsRequired();
        builder.Property(x => x.Exchange).HasMaxLength(16);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();

        // Stored as text so the external ML pipeline reads meaningful values, not ordinals.
        builder.Property(x => x.AssetType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(x => x.Symbol).IsUnique();
        builder.HasIndex(x => x.IsTracked).HasFilter("is_tracked");
        builder.HasIndex(x => x.CompanyId);

        builder.HasOne(x => x.Company)
            .WithMany(x => x.Stocks)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.DailyPrices).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

/// <summary>
/// The eleven GICS sectors, seeded so the universe file can reference sectors by key without
/// a bootstrapping step. Ids are fixed because migrations reference them.
/// </summary>
internal static class SectorSeed
{
    public static IReadOnlyList<object> Rows { get; } =
    [
        new { Id = 1, Key = "Energy", Name = "Energy", DisplayOrder = 1 },
        new { Id = 2, Key = "Materials", Name = "Materials", DisplayOrder = 2 },
        new { Id = 3, Key = "Industrials", Name = "Industrials", DisplayOrder = 3 },
        new { Id = 4, Key = "ConsumerDiscretionary", Name = "Consumer Discretionary", DisplayOrder = 4 },
        new { Id = 5, Key = "ConsumerStaples", Name = "Consumer Staples", DisplayOrder = 5 },
        new { Id = 6, Key = "HealthCare", Name = "Health Care", DisplayOrder = 6 },
        new { Id = 7, Key = "Financials", Name = "Financials", DisplayOrder = 7 },
        new { Id = 8, Key = "InformationTechnology", Name = "Information Technology", DisplayOrder = 8 },
        new { Id = 9, Key = "CommunicationServices", Name = "Communication Services", DisplayOrder = 9 },
        new { Id = 10, Key = "Utilities", Name = "Utilities", DisplayOrder = 10 },
        new { Id = 11, Key = "RealEstate", Name = "Real Estate", DisplayOrder = 11 },
    ];
}
