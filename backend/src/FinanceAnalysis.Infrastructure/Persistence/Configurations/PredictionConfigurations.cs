using FinanceAnalysis.Domain.Predictions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceAnalysis.Infrastructure.Persistence.Configurations;

/// <summary>
/// Schema for the tables owned by MLPipeline_Jordan.
/// </summary>
/// <remarks>
/// This application creates and migrates these tables so the pipeline has a stable contract
/// to write into, then reads them and nothing more. Enum columns are stored as text
/// specifically so the Python side can write <c>'Buy'</c> rather than having to mirror a C#
/// ordinal, and every measurement column is nullable because different model families emit
/// different subsets.
/// </remarks>
internal sealed class MlModelConfiguration : IEntityTypeConfiguration<MlModel>
{
    public void Configure(EntityTypeBuilder<MlModel> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Version).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(x => new { x.Key, x.Version }).IsUnique();

        builder.Navigation(x => x.Predictions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class MlPredictionConfiguration : IEntityTypeConfiguration<MlPrediction>
{
    public void Configure(EntityTypeBuilder<MlPrediction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PredictedClose).HasPrecision(18, 6);
        builder.Property(x => x.PredictedReturn).HasPrecision(18, 8);
        builder.Property(x => x.Confidence).HasPrecision(9, 8);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

        builder.Property(x => x.Direction).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(x => x.Signal).HasConversion<string>().HasMaxLength(16).IsRequired();

        builder.HasIndex(x => new { x.ModelId, x.StockId, x.PredictionDate, x.TargetDate })
            .IsUnique()
            .HasDatabaseName("ux_ml_predictions_model_stock_dates");

        builder.HasIndex(x => new { x.StockId, x.PredictionDate }).IsDescending(false, true);

        builder.ToTable(t =>
            t.HasCheckConstraint("ck_ml_predictions_confidence", "confidence IS NULL OR (confidence >= 0 AND confidence <= 1)"));

        builder.HasOne(x => x.Model)
            .WithMany(x => x.Predictions)
            .HasForeignKey(x => x.ModelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Stock)
            .WithMany()
            .HasForeignKey(x => x.StockId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.History).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class MlPredictionHistoryConfiguration : IEntityTypeConfiguration<MlPredictionHistory>
{
    public void Configure(EntityTypeBuilder<MlPredictionHistory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PredictedValue).HasPrecision(18, 6);
        builder.Property(x => x.ActualValue).HasPrecision(18, 6);
        builder.Property(x => x.AbsoluteError).HasPrecision(18, 6);
        builder.Property(x => x.PercentageError).HasPrecision(18, 8);
        builder.Property(x => x.EvaluatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(x => x.PredictionId).IsUnique();
        builder.HasIndex(x => new { x.ModelId, x.TargetDate });

        builder.HasOne(x => x.Prediction)
            .WithMany(x => x.History)
            .HasForeignKey(x => x.PredictionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Model)
            .WithMany()
            .HasForeignKey(x => x.ModelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Stock)
            .WithMany()
            .HasForeignKey(x => x.StockId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
