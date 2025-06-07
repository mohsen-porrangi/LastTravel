using System.Security.Claims;

namespace UserManagement.API.Common.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetIdentityId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue("MasterIdentityId")
                         ?? throw new UnauthorizedAccessException("IdentityId not found in token");

            return Guid.Parse(value);
        }
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue("UserId")
                         ?? throw new UnauthorizedAccessException("UserId not found in token");

            return Guid.Parse(value);
        }
    }
}
