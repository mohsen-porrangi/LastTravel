using BuildingBlocks.Contracts;
using BuildingBlocks.Domain;

namespace UserManagement.API.Infrastructure.Data.Models
{
    public class MasterIdentity : BaseEntity<Guid>, ISoftDelete
    {
        public string? Email { get; set; } = null;
        public string Mobile { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public bool IsLocked { get; set; }
        public int LoginAttempt { get; set; }
        public string? RefreshToken { get; set; } = default!;
        public DateTime? LastLogin { get; set; }

        public Guid? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
        public bool IsActive { get; set; } = true;

    }

}
