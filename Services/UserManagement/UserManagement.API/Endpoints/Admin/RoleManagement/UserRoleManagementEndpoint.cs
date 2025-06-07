using UserManagement.API.Endpoints.Admin.RoleManagement.AssignRole;
using UserManagement.API.Endpoints.Admin.RoleManagement.GetUserRoles;
using UserManagement.API.Endpoints.Admin.RoleManagement.UnassignRole;

namespace UserManagement.API.Endpoints.Admin.RoleManagement;

public class UserRoleManagementEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // دریافت نقش‌های یک کاربر
        app.MapGet("api/users/{userId:guid}/roles", async (
            Guid userId,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetUserRolesQuery(userId);
            var result = await sender.Send(query, ct);
            return Results.Ok(result);
        })
            .WithName("GetUserRoles")
            .WithDescription("دریافت لیست نقش‌های اختصاص داده شده به کاربر")
            .Produces<GetUserRolesResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Users")
            .RequireAuthorization("Admin");

        // اختصاص نقش به کاربر
        app.MapPost("api/users/{userId:guid}/roles", async (
            Guid userId,
            AssignRoleToUserCommand body,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = body with { UserId = userId };
            await sender.Send(command, ct);
            return Results.NoContent();
        })
            .WithName("AssignRoleToUser")
            .WithDescription("اختصاص نقش جدید به کاربر")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Users")
            .RequireAuthorization("Admin");

        // حذف نقش از کاربر
        app.MapDelete("api/users/{userId:guid}/roles/{roleId:int}", async (
            Guid userId,
            int roleId,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new UnassignRoleFromUserCommand(userId, roleId);
            await sender.Send(command, ct);
            return Results.NoContent();
        })
            .WithName("UnassignRoleFromUser")
            .WithDescription("حذف نقش از کاربر")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Users")
            .RequireAuthorization("Admin");
    }
}