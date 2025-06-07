using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WalletPayment.Domain.Aggregates.WalletAggregate;
using WalletPayment.Domain.Enums;
using WalletPayment.Domain.ValueObjects;

namespace WalletPayment.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for CurrencyAccount entity
/// </summary>
public class CurrencyAccountConfiguration : IEntityTypeConfiguration<CurrencyAccount>
{
    public void Configure(EntityTypeBuilder<CurrencyAccount> builder)
    {
        // Table configuration
        builder.ToTable("CurrencyAccounts");
        builder.HasKey(ca => ca.Id);

        // Properties
        builder.Property(ca => ca.Id)
            .IsRequired();

        builder.Property(ca => ca.WalletId)
            .IsRequired();

        builder.Property(ca => ca.Currency)
            .HasConversion<int>()
            .IsRequired()
            .HasComment("Currency code (1=IRR, 2=USD, etc.)");

        // Configure Money value object
        builder.OwnsOne(ca => ca.Balance, money =>
        {
            money.Property(m => m.Value)
                .HasColumnName("Balance")
                .HasColumnType("decimal(18,2)")
                .IsRequired()
                .HasDefaultValue(0);

            money.Property(m => m.Currency)
                .HasColumnName("BalanceCurrency")
                .HasConversion<int>()
                .IsRequired();
        });

        builder.Property(ca => ca.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ca => ca.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(ca => ca.UpdatedAt)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(ca => ca.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(ca => new { ca.WalletId, ca.Currency })
            .IsUnique()
            .HasDatabaseName("IX_CurrencyAccounts_WalletId_Currency");

        builder.HasIndex(ca => ca.IsActive)
            .HasDatabaseName("IX_CurrencyAccounts_IsActive");

        builder.HasIndex(ca => ca.IsDeleted)
            .HasDatabaseName("IX_CurrencyAccounts_IsDeleted");

        // Relationships
        builder.HasOne(ca => ca.Wallet)
            .WithMany(w => w.CurrencyAccounts)
            .HasForeignKey(ca => ca.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(ca => ca.DomainEvents);
    }
}