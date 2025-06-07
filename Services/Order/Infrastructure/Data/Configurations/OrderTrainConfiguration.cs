using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Entities;

namespace Order.Infrastructure.Data.Configurations;

public class OrderTrainConfiguration : IEntityTypeConfiguration<OrderTrain>
{
    public void Configure(EntityTypeBuilder<OrderTrain> builder)
    {
        builder.ToTable("OrderTrains");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.FirstNameEn).HasMaxLength(100).IsRequired();
        builder.Property(t => t.LastNameEn).HasMaxLength(100).IsRequired();
        builder.Property(t => t.FirstNameFa).HasMaxLength(100).IsRequired();
        builder.Property(t => t.LastNameFa).HasMaxLength(100).IsRequired();

        builder.Property(t => t.NationalCode).HasMaxLength(10);
        builder.Property(t => t.PassportNumber).HasMaxLength(50);

        builder.Property(t => t.SourceName).HasMaxLength(100);
        builder.Property(t => t.DestinationName).HasMaxLength(100);

        builder.Property(t => t.TrainNumber).HasMaxLength(20).IsRequired();

        builder.Property(t => t.Gender)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(t => t.AgeGroup)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.TicketDirection)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.ProviderId)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.BasePrice).HasPrecision(18, 2);
        builder.Property(t => t.Tax).HasPrecision(18, 2);
        builder.Property(t => t.Fee).HasPrecision(18, 2);

        builder.Property(t => t.PNR).HasMaxLength(20);
        builder.Property(t => t.TicketNumber).HasMaxLength(50);
        builder.Property(t => t.SeatNumber).HasMaxLength(20);

        builder.HasIndex(t => t.TicketNumber);
        builder.HasIndex(t => t.OrderId);
    }
}