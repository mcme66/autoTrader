using FinanceAnalysis.Domain.Identity;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceAnalysis.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.NormalizedEmail).HasMaxLength(256).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(256);
        builder.Property(x => x.DisplayName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => x.NormalizedEmail).IsUnique();

        builder.Navigation(x => x.UserRoles).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.ExternalLogins).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.RefreshTokens).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Portfolios).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(64).IsRequired();
        builder.Property(x => x.NormalizedName).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(256).IsRequired();

        builder.HasIndex(x => x.NormalizedName).IsUnique();

        builder.Navigation(x => x.UserRoles).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasData(
            new
            {
                Id = 1,
                Name = RoleNames.Administrator,
                NormalizedName = "ADMINISTRATOR",
                Description = "Full access, including internal ingestion endpoints and universe management.",
            },
            new
            {
                Id = 2,
                Name = RoleNames.Member,
                NormalizedName = "MEMBER",
                Description = "Standard access to market data, portfolios and recommendations.",
            });
    }
}

internal sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(x => new { x.UserId, x.RoleId });

        builder.HasOne(x => x.User)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
{
    public void Configure(EntityTypeBuilder<ExternalLogin> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Provider).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ProviderKey).HasMaxLength(256).IsRequired();

        builder.HasIndex(x => new { x.Provider, x.ProviderKey }).IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(x => x.ExternalLogins)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        // Base64 of a SHA-256 digest.
        builder.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ReplacedByTokenHash).HasMaxLength(64);
        builder.Property(x => x.CreatedByIp).HasMaxLength(64);

        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.ExpiresAt });

        builder.HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
