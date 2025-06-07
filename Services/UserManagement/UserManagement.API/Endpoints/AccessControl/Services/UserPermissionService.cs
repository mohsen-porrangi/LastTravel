using BuildingBlocks.Contracts.Security;
using UserManagement.API.Infrastructure.Data;

namespace UserManagement.API.Endpoints.AccessControl.Services;

public class UserPermissionService : IPermissionService
{
    private readonly UserDbContext _db;

    public UserPermissionService(UserDbContext db)
    {
        _db = db;
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        var permissions = await _db.UserRoles
           .Where(ur => ur.UserId == userId)
           .SelectMany(ur => _db.RolePermissions
               .Where(rp => rp.RoleId == ur.RoleId)
               .Select(rp => rp.Permission.Code))
           .Distinct()
           .ToListAsync();

        return permissions;
    }
}
