using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Entities;

namespace Order.Infrastructure.Data.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("OrderStatusHistory");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.FromStatus)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(h => h.ToStatus)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(h => h.Reason)
            .HasMaxLength(500);

        builder.HasIndex(h => h.OrderId);
        builder.HasIndex(h => h.CreatedAt);
    }
}