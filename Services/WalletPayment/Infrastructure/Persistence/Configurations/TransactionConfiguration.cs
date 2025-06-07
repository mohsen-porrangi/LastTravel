using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WalletPayment.Domain.TransactionAggregate;
using WalletPayment.Domain.Enums;
using WalletPayment.Domain.Aggregates.TransactionAggregate;

namespace WalletPayment.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Transaction entity
/// ✅ FINAL FIX: Removed duplicate TransactionNumber configuration
/// </summary>
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        // Table configuration
        builder.ToTable("Transactions");
        builder.HasKey(t => t.Id);

        // Properties
        builder.Property(t => t.Id)
            .IsRequired();

        // ✅ Configure TransactionNumber value object - ONLY ONCE
        builder.OwnsOne(t => t.TransactionNumber, tn =>
        {
            tn.Property(x => x.Value)
                .HasColumnName("TransactionNumber")
                .HasMaxLength(50)
                .IsRequired();

            // ✅ Index on the column name directly
            tn.HasIndex(x => x.Value)
                .IsUnique()
                .HasDatabaseName("IX_Transactions_TransactionNumber");
        });

        builder.Property(t => t.WalletId)
            .IsRequired();

        builder.Property(t => t.CurrencyAccountId)
            .IsRequired();

        builder.Property(t => t.UserId)
            .IsRequired()
            .HasComment("User who initiated the transaction");

        builder.Property(t => t.RelatedTransactionId)
            .IsRequired(false)
            .HasComment("Link to related transaction (refunds, transfers)");

        // ✅ Configure Money value object
        builder.OwnsOne(t => t.Amount, money =>
        {
            money.Property(m => m.Value)
                .HasColumnName("Amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasConversion<int>()
                .IsRequired();
        });

        builder.Property(t => t.Direction)
            .HasConversion<int>()
            .IsRequired()
            .HasComment("1=In, 2=Out");

        builder.Property(t => t.Type)
            .HasConversion<int>()
            .IsRequired()
            .HasComment("1=Deposit, 2=Withdrawal, 3=Purchase, 4=Refund, etc.");

        builder.Property(t => t.Status)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(TransactionStatus.Pending)
            .HasComment("1=Pending, 2=Completed, 3=Processing, 4=Failed, etc.");

        builder.Property(t => t.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(t => t.IsCredit)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Is this a credit transaction (B2B)");

        builder.Property(t => t.DueDate)
            .HasColumnType("datetime2(7)")
            .IsRequired(false)
            .HasComment("Due date for credit transactions");

        builder.Property(t => t.PaymentReferenceId)
            .HasMaxLength(100)
            .IsRequired(false)
            .HasComment("Payment gateway reference ID");

        builder.Property(t => t.OrderContext)
            .HasMaxLength(100)
            .IsRequired(false)
            .HasComment("Order ID or context information");

        builder.Property(t => t.TransactionDate)
            .IsRequired()
            .HasColumnType("datetime2(7)")
            .HasComment("When transaction was initiated");

        builder.Property(t => t.ProcessedAt)
            .HasColumnType("datetime2(7)")
            .IsRequired(false)
            .HasComment("When transaction was completed");

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(t => t.UpdatedAt)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // ✅ Other indexes - NO TransactionNumber here since it's defined in OwnsOne
        builder.HasIndex(t => t.WalletId)
            .HasDatabaseName("IX_Transactions_WalletId");

        builder.HasIndex(t => t.CurrencyAccountId)
            .HasDatabaseName("IX_Transactions_CurrencyAccountId");

        builder.HasIndex(t => t.UserId)
            .HasDatabaseName("IX_Transactions_UserId");

        builder.HasIndex(t => t.PaymentReferenceId)
            .HasDatabaseName("IX_Transactions_PaymentReferenceId");

        builder.HasIndex(t => t.Status)
            .HasDatabaseName("IX_Transactions_Status");

        builder.HasIndex(t => t.Type)
            .HasDatabaseName("IX_Transactions_Type");

        builder.HasIndex(t => t.TransactionDate)
            .HasDatabaseName("IX_Transactions_TransactionDate");

        builder.HasIndex(t => new { t.UserId, t.TransactionDate })
            .HasDatabaseName("IX_Transactions_UserId_TransactionDate");

        // Relationships
        builder.HasOne(t => t.Wallet)
            .WithMany()
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.CurrencyAccount)
            .WithMany(ca => ca.Transactions)
            .HasForeignKey(t => t.CurrencyAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing relationship for related transactions
        builder.HasOne<Transaction>()
            .WithMany()
            .HasForeignKey(t => t.RelatedTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore domain events
        builder.Ignore(t => t.DomainEvents);
    }
}