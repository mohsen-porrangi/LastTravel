using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Entities;

namespace Order.Infrastructure.Data.Configurations;

public class OrderFlightConfiguration : IEntityTypeConfiguration<OrderFlight>
{
    public void Configure(EntityTypeBuilder<OrderFlight> builder)
    {
        builder.ToTable("OrderFlights");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.FirstNameEn).HasMaxLength(100).IsRequired();
        builder.Property(f => f.LastNameEn).HasMaxLength(100).IsRequired();
        builder.Property(f => f.FirstNameFa).HasMaxLength(100).IsRequired();
        builder.Property(f => f.LastNameFa).HasMaxLength(100).IsRequired();

        builder.Property(f => f.NationalCode).HasMaxLength(10);
        builder.Property(f => f.PassportNumber).HasMaxLength(50);

        builder.Property(f => f.SourceName).HasMaxLength(100);
        builder.Property(f => f.DestinationName).HasMaxLength(100);

        builder.Property(f => f.FlightNumber).HasMaxLength(20).IsRequired();

        builder.Property(f => f.Gender)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(f => f.AgeGroup)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(f => f.TicketDirection)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(f => f.ProviderId)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(f => f.BasePrice).HasPrecision(18, 2);
        builder.Property(f => f.Tax).HasPrecision(18, 2);
        builder.Property(f => f.Fee).HasPrecision(18, 2);

        builder.Property(f => f.PNR).HasMaxLength(20);
        builder.Property(f => f.TicketNumber).HasMaxLength(50);
        builder.Property(f => f.SeatNumber).HasMaxLength(10);

        builder.HasIndex(f => f.TicketNumber);
        builder.HasIndex(f => f.OrderId);
    }
}