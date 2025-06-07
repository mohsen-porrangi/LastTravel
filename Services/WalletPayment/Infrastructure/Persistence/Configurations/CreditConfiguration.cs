using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WalletPayment.Domain.Aggregates.WalletAggregate;
using WalletPayment.Domain.Enums;

namespace WalletPayment.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Credit entity (B2B)
/// </summary>
public class CreditConfiguration : IEntityTypeConfiguration<Credit>
{
    public void Configure(EntityTypeBuilder<Credit> builder)
    {
        // Table configuration
        builder.ToTable("Credits");
        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.Id)
            .IsRequired();

        builder.Property(c => c.WalletId)
            .IsRequired();

        // Configure CreditLimit Money value object
        builder.OwnsOne(c => c.CreditLimit, money =>
        {
            money.Property(m => m.Value)
                .HasColumnName("CreditLimit")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("CreditLimitCurrency")
                .HasConversion<int>()
                .IsRequired();
        });

        // Configure UsedCredit Money value object
        builder.OwnsOne(c => c.UsedCredit, money =>
        {
            money.Property(m => m.Value)
                .HasColumnName("UsedCredit")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("UsedCreditCurrency")
                .HasConversion<int>()
                .IsRequired();
        });

        builder.Property(c => c.GrantedDate)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(c => c.DueDate)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(c => c.SettledDate)
            .HasColumnType("datetime2(7)")
            .IsRequired(false);

        builder.Property(c => c.Status)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(CreditStatus.Active);

        builder.Property(c => c.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(c => c.SettlementTransactionId)
            .IsRequired(false);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(c => c.UpdatedAt)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(c => c.WalletId)
            .HasDatabaseName("IX_Credits_WalletId");

        builder.HasIndex(c => c.Status)
            .HasDatabaseName("IX_Credits_Status");

        builder.HasIndex(c => c.DueDate)
            .HasDatabaseName("IX_Credits_DueDate");

        builder.HasIndex(c => new { c.Status, c.DueDate })
            .HasDatabaseName("IX_Credits_Status_DueDate");
    }
}