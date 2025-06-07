using BuildingBlocks.Contracts;
using BuildingBlocks.Domain;

namespace UserManagement.API.Infrastructure.Data.Models
{
    public class Role : BaseEntity<int>, ISoftDelete
    {
        public string Name { get; set; } = default!;

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

}
