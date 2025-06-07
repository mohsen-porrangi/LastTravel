using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserManagement.API.Infrastructure.Data.Models.Configurations
{
    public class MasterIdentityConfiguration : IEntityTypeConfiguration<MasterIdentity>
    {
        public void Configure(EntityTypeBuilder<MasterIdentity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Mobile).IsRequired();
            builder.Property(x => x.PasswordHash).IsRequired();

            builder.HasIndex(x => x.Mobile).IsUnique();
        }
    }
}
