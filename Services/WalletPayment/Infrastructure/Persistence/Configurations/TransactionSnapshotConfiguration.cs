using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WalletPayment.Domain.TransactionAggregate;

namespace WalletPayment.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for TransactionSnapshot entity
/// </summary>
public class TransactionSnapshotConfiguration : IEntityTypeConfiguration<TransactionSnapshot>
{
    public void Configure(EntityTypeBuilder<TransactionSnapshot> builder)
    {
        // Table configuration
        builder.ToTable("TransactionSnapshots");
        builder.HasKey(ts => ts.Id);

        // Properties
        builder.Property(ts => ts.Id)
            .IsRequired();

        builder.Property(ts => ts.AccountId)
            .IsRequired()
            .HasComment("Currency account ID");

        // Configure Balance Money value object
        builder.OwnsOne(ts => ts.Balance, money =>
        {
            money.Property(m => m.Value)
                .HasColumnName("Balance")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasConversion<int>()
                .IsRequired();
        });

        builder.Property(ts => ts.SnapshotDate)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(ts => ts.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(ts => ts.TransactionId)
            .IsRequired(false)
            .HasComment("Optional link to specific transaction");

        // Indexes
        builder.HasIndex(ts => ts.AccountId)
            .HasDatabaseName("IX_TransactionSnapshots_AccountId");

        builder.HasIndex(ts => ts.SnapshotDate)
            .HasDatabaseName("IX_TransactionSnapshots_SnapshotDate");

        builder.HasIndex(ts => new { ts.AccountId, ts.SnapshotDate })
            .HasDatabaseName("IX_TransactionSnapshots_AccountId_SnapshotDate");

        builder.HasIndex(ts => ts.Type)
            .HasDatabaseName("IX_TransactionSnapshots_Type");
    }
}