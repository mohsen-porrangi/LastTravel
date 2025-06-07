using BuildingBlocks.Contracts;
using BuildingBlocks.Domain;

namespace UserManagement.API.Infrastructure.Data.Models
{
    public class UserRole : BaseEntity<int>, ISoftDelete
    {
        public Guid UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime AssignedAt { get; set; }

        public virtual User User { get; set; } = default!;
        public virtual Role Role { get; set; } = default!;
    }

}
