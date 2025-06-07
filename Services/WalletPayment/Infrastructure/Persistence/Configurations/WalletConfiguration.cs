using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WalletPayment.Domain.Aggregates.WalletAggregate;
using WalletPayment.Domain.Enums;

namespace WalletPayment.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Wallet entity
/// </summary>
public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        // Table configuration
        builder.ToTable("Wallets");
        builder.HasKey(w => w.Id);

        // Properties
        builder.Property(w => w.Id)
            .HasConversion(
                id => id,
                value => value)
            .IsRequired();

        builder.Property(w => w.UserId)
            .IsRequired()
            .HasComment("User who owns this wallet");

        builder.Property(w => w.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(w => w.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(w => w.UpdatedAt)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(w => w.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(w => w.UserId)
            .IsUnique()
            .HasDatabaseName("IX_Wallets_UserId");

        builder.HasIndex(w => w.IsActive)
            .HasDatabaseName("IX_Wallets_IsActive");

        builder.HasIndex(w => w.IsDeleted)
            .HasDatabaseName("IX_Wallets_IsDeleted");

        // Relationships
        builder.HasMany(w => w.CurrencyAccounts)
            .WithOne(ca => ca.Wallet)
            .HasForeignKey(ca => ca.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.BankAccounts)
            .WithOne(ba => ba.Wallet)
            .HasForeignKey(ba => ba.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Credits)
            .WithOne()
            .HasForeignKey(c => c.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events (they are not persisted)
        builder.Ignore(w => w.DomainEvents);
    }
}