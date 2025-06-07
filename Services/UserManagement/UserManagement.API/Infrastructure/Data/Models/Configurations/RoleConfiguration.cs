using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserManagement.API.Infrastructure.Data.Models.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired();

            builder.HasMany(x => x.UserRoles)
                   .WithOne(x => x.Role)
                   .HasForeignKey(x => x.RoleId);

            builder.HasMany(x => x.RolePermissions)
                   .WithOne(x => x.Role)
                   .HasForeignKey(x => x.RoleId);
        }
    }
}
