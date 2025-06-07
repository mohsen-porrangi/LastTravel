using UserManagement.API.Infrastructure.Data;

namespace UserManagement.API.Endpoints.Admin.RoleManagement.GetRolePermissions;

internal sealed class GetRolePermissionsQueryHandler(UserDbContext db)
    : IQueryHandler<GetRolePermissionsQuery, GetRolePermissionsResult>
{
    public async Task<GetRolePermissionsResult> Handle(GetRolePermissionsQuery query, CancellationToken cancellationToken)
    {
        var permissions = await db.RolePermissions
            .Where(rp => rp.RoleId == query.RoleId)
            .Include(rp => rp.Permission)
            .Select(rp => new PermissionDto(
                rp.Permission.Id,
                rp.Permission.Module,
                rp.Permission.Action,
                rp.Permission.Code,
                rp.Permission.Description
            ))
            .ToListAsync(cancellationToken);

        return new GetRolePermissionsResult(permissions);
    }
}
