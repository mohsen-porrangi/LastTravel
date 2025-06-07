using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Entities;

namespace Order.Infrastructure.Data.Configurations;

public class SavedPassengerConfiguration : IEntityTypeConfiguration<SavedPassenger>
{
    public void Configure(EntityTypeBuilder<SavedPassenger> builder)
    {
        builder.ToTable("SavedPassengers");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.FirstNameEn).HasMaxLength(100).IsRequired();
        builder.Property(p => p.LastNameEn).HasMaxLength(100).IsRequired();
        builder.Property(p => p.FirstNameFa).HasMaxLength(100).IsRequired();
        builder.Property(p => p.LastNameFa).HasMaxLength(100).IsRequired();

        builder.Property(p => p.NationalCode).HasMaxLength(10).IsRequired();
        builder.Property(p => p.PassportNumber).HasMaxLength(50);

        builder.Property(p => p.Gender)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.HasIndex(p => new { p.UserId, p.NationalCode })
            .IsUnique();

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}