using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Entities;

namespace Order.Infrastructure.Data.Configurations;

public class OrderTrainCarTransportConfiguration : IEntityTypeConfiguration<OrderTrainCarTransport>
{
    public void Configure(EntityTypeBuilder<OrderTrainCarTransport> builder)
    {
        builder.ToTable("OrderTrainCarTransports");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CarNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.CarName)
            .IsRequired()
            .HasMaxLength(100);

        // Inherit other configurations from base OrderItem
        builder.Property(c => c.FirstNameEn)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.LastNameEn)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.FirstNameFa)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.LastNameFa)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.BasePrice)
            .HasPrecision(18, 2);

        builder.Property(c => c.Tax)
            .HasPrecision(18, 2);

        builder.Property(c => c.Fee)
            .HasPrecision(18, 2);

        builder.HasIndex(c => c.OrderId);
    }
}