using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserManagement.API.Infrastructure.Data.Models.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired(false);
            builder.Property(x => x.Family).IsRequired(false);
            builder.Property(x => x.NationalCode).IsRequired(false);
            builder.Property(x => x.Gender).IsRequired(false);
            builder.HasOne(u => u.MasterIdentity)
                .WithOne()
                .HasForeignKey<User>(u => u.IdentityId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            builder.HasIndex(x => x.NationalCode)
                .IsUnique();

            builder.HasMany(x => x.Roles)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId);
        }
    }
}
