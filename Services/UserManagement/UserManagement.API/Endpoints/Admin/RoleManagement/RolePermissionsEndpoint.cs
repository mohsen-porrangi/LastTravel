using UserManagement.API.Endpoints.Admin.RoleManagement.AssignPermission;
using UserManagement.API.Endpoints.Admin.RoleManagement.GetRolePermissions;
using UserManagement.API.Endpoints.Admin.RoleManagement.UnassignPermission;

namespace UserManagement.API.Endpoints.Admin.RoleManagement;

public class RolePermissionsEndpoint : ICarterModule
{
    //[RequirePermission(RolePermissions.Read)]
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // دریافت دسترسی‌های یک نقش
        app.MapGet("api/roles/{roleId:int}/permissions", async (
            int roleId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetRolePermissionsQuery(roleId), ct);
            return Results.Ok(result);
        })
            .WithName("GetPermissionsForRole")
            .WithDescription("دریافت لیست دسترسی‌های یک نقش")
            .Produces<GetRolePermissionsResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("Roles")
            .RequireAuthorization("Admin");

        // افزودن دسترسی به نقش
        app.MapPost("api/roles/{roleId:int}/permissions", async (
            int roleId,
            AssignPermissionCommand body,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = body with { RoleId = roleId };
            await sender.Send(command, ct);
            return Results.NoContent();
        })
            .WithName("AddPermissionToRole")
            .WithDescription("اختصاص دسترسی به یک نقش")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("Roles")
            .RequireAuthorization("Admin");

        // حذف دسترسی از نقش
        app.MapDelete("api/roles/{roleId:int}/permissions/{permissionId:int}", async (
            int roleId,
            int permissionId,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new UnassignPermissionCommand(roleId, permissionId);
            await sender.Send(command, ct);
            return Results.NoContent();
        })
            .WithName("DeletePermissionFromRole")
            .WithDescription("حذف دسترسی از یک نقش")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("Roles")
            .RequireAuthorization("Admin");
    }
}