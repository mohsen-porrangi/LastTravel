using BuildingBlocks.Contracts;
using BuildingBlocks.Domain;

namespace UserManagement.API.Infrastructure.Data.Models
{
    public class LoginHistory : BaseEntity<int>, ISoftDelete
    {
        public Guid IdentityId { get; set; }
        public string Username { get; set; } = default!;
        public string IpAddress { get; set; } = default!;
        public string DeviceInfo { get; set; } = default!;
        public DateTime LoginTime { get; set; }
        public bool Success { get; set; }
        public string? FailReason { get; set; }
    }
}
