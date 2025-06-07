using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserManagement.API.Infrastructure.Data.Models.Configurations
{
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}
