using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WalletPayment.Domain.Aggregates.WalletAggregate;

namespace WalletPayment.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for BankAccount entity
/// </summary>
public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        // Table configuration
        builder.ToTable("BankAccounts");
        builder.HasKey(ba => ba.Id);

        // Properties
        builder.Property(ba => ba.Id)
            .IsRequired();

        builder.Property(ba => ba.WalletId)
            .IsRequired();

        builder.Property(ba => ba.BankName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ba => ba.AccountNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(ba => ba.CardNumber)
            .HasMaxLength(16)
            .IsRequired(false);

        builder.Property(ba => ba.ShabaNumber)
            .HasMaxLength(26)
            .IsRequired(false);

        builder.Property(ba => ba.AccountHolderName)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(ba => ba.IsVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ba => ba.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ba => ba.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ba => ba.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(ba => ba.UpdatedAt)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(ba => ba.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(ba => new { ba.WalletId, ba.AccountNumber })
            .IsUnique()
            .HasDatabaseName("IX_BankAccounts_WalletId_AccountNumber");

        builder.HasIndex(ba => ba.IsDefault)
            .HasDatabaseName("IX_BankAccounts_IsDefault");

        builder.HasIndex(ba => ba.IsActive)
            .HasDatabaseName("IX_BankAccounts_IsActive");

        builder.HasIndex(ba => ba.IsDeleted)
            .HasDatabaseName("IX_BankAccounts_IsDeleted");

        // Relationships
        builder.HasOne(ba => ba.Wallet)
            .WithMany(w => w.BankAccounts)
            .HasForeignKey(ba => ba.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(ba => ba.DomainEvents);
    }
}
