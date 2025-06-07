using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Order.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Domain.Entities.Order>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.Property(o => o.ServiceType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.LastStatus)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.FullAmount)
            .HasPrecision(18, 2);

        builder.HasMany(o => o.OrderFlights)
            .WithOne(f => f.Order)
            .HasForeignKey(f => f.OrderId);

        builder.HasMany(o => o.OrderTrains)
            .WithOne(t => t.Order)
            .HasForeignKey(t => t.OrderId);

        builder.HasMany(o => o.OrderTrainCarTransports)
            .WithOne(c => c.Order)
            .HasForeignKey(c => c.OrderId);

        builder.HasMany(o => o.StatusHistories)
            .WithOne(s => s.Order)
            .HasForeignKey(s => s.OrderId);

        builder.HasMany(o => o.WalletTransactions)
            .WithOne(w => w.Order)
            .HasForeignKey(w => w.OrderId);
    }
}