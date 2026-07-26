using FinanceAnalysis.Domain.MarketData;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceAnalysis.Infrastructure.Persistence.Configurations;

internal sealed class DataSourceConfiguration : IEntityTypeConfiguration<DataSource>
{
    public void Configure(EntityTypeBuilder<DataSource> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();

        builder.HasIndex(x => x.Key).IsUnique();

        builder.HasData(
            new { Id = 1, Key = "polygon", Name = "Polygon.io" },
            new { Id = 2, Key = "mock", Name = "Deterministic Mock Provider" });
    }
}

/// <summary>
/// Append-only OHLCV storage.
/// </summary>
/// <remarks>
/// The unique <c>(stock_id, trade_date)</c> index is the mechanism that enforces
/// "history is never overwritten": inserts use <c>ON CONFLICT DO NOTHING</c> against it, so a
/// repeated ingestion silently skips rows that already exist instead of updating them.
///
/// Indexing strategy: the composite descending index serves "give me this symbol's recent
/// bars", which is the dominant read. The BRIN index on <c>trade_date</c> covers whole-market
/// scans for a date at a fraction of the size of a B-tree, which suits a table that is only
/// ever appended to in date order. At roughly 300 symbols x 252 trading days the table grows
/// by well under 100k rows per year, so partitioning would cost more than it saves; if the
/// universe grows by two orders of magnitude, range-partitioning by year on trade_date is the
/// natural next step and does not change this entity.
/// </remarks>
internal sealed class DailyPriceConfiguration : IEntityTypeConfiguration<DailyPrice>
{
    public void Configure(EntityTypeBuilder<DailyPrice> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Open).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.High).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.Low).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.Close).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.VolumeWeightedAveragePrice).HasPrecision(18, 6);
        builder.Property(x => x.Volume).IsRequired();

        // Serves both duplicate rejection and "this symbol's most recent bars"; Postgres
        // scans a B-tree backwards, so no separate descending index is warranted.
        builder.HasIndex(x => new { x.StockId, x.TradeDate })
            .IsUnique()
            .HasDatabaseName("ux_daily_prices_stock_trade_date");

        builder.HasIndex(x => x.TradeDate)
            .HasMethod("brin")
            .HasDatabaseName("ix_daily_prices_trade_date_brin");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_daily_prices_high_low", "high >= low");
            t.HasCheckConstraint("ck_daily_prices_volume", "volume >= 0");
            t.HasCheckConstraint("ck_daily_prices_positive", "open > 0 AND high > 0 AND low > 0 AND close > 0");
        });

        builder.HasOne(x => x.Stock)
            .WithMany(x => x.DailyPrices)
            .HasForeignKey(x => x.StockId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DataSource)
            .WithMany()
            .HasForeignKey(x => x.DataSourceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class IngestionRunConfiguration : IEntityTypeConfiguration<IngestionRun>
{
    public void Configure(EntityTypeBuilder<IngestionRun> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.RunType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

        builder.HasIndex(x => new { x.RunType, x.TradeDate, x.Status });
        builder.HasIndex(x => x.QueuedAt).IsDescending();

        builder.HasOne(x => x.DataSource)
            .WithMany()
            .HasForeignKey(x => x.DataSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.Duration);
    }
}
