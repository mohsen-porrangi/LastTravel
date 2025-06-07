using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Entities;

namespace Order.Infrastructure.Data.Configurations;

public class OrderWalletTransactionConfiguration : IEntityTypeConfiguration<OrderWalletTransaction>
{
    public void Configure(EntityTypeBuilder<OrderWalletTransaction> builder)
    {
        builder.ToTable("OrderWalletTransactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.Amount)
            .HasPrecision(18, 2);

        builder.HasIndex(t => t.OrderId);
        builder.HasIndex(t => t.TransactionId);
    }
}