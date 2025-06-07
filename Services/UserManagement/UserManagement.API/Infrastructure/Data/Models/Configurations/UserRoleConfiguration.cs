using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserManagement.API.Infrastructure.Data.Models.Configurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.AssignedAt).IsRequired();
        }
    }
}
