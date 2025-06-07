using UserManagement.API.Endpoints.Admin.RoleManagement.Role.CreateRole;
using UserManagement.API.Endpoints.Admin.RoleManagement.Role.DeleteRole;
using UserManagement.API.Endpoints.Admin.RoleManagement.Role.GetAllRoles;
using UserManagement.API.Endpoints.Admin.RoleManagement.Role.UpdateRole;

namespace UserManagement.API.Endpoints.Admin.RoleManagement;

public class RolesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // دریافت همه نقش‌ها
        app.MapGet("api/roles", async (
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAllRolesQuery(), ct);
            return Results.Ok(result);
        })
            .WithName("ListAllRoles")
            .WithDescription("دریافت لیست تمام نقش‌ها")
            .Produces<GetAllRolesResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("Roles")
            .RequireAuthorization("Admin");

        // ایجاد نقش جدید
        app.MapPost("api/roles", async (
            CreateRoleCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var id = await sender.Send(command, ct);
            return Results.Created($"/roles/{id}", new { id });
        })
            .WithName("CreateNewRole")
            .WithDescription("ایجاد نقش جدید")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithTags("Roles")
            .RequireAuthorization("Admin");

        // بروزرسانی نقش موجود
        app.MapPut("api/roles/{id:int}", async (
            int id,
            UpdateRoleCommand body,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = body with { Id = id };
            await sender.Send(command, ct);
            return Results.NoContent();
        })
            .WithName("UpdateExistingRole")
            .WithDescription("بروزرسانی اطلاعات نقش موجود")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithTags("Roles")
            .RequireAuthorization("Admin");

        // حذف نقش
        app.MapDelete("api/roles/{id:int}", async (
            int id,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new DeleteRoleCommand(id), ct);
            return Results.NoContent();
        })
            .WithName("RemoveRole")
            .WithDescription("حذف نقش")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("Roles")
            .RequireAuthorization("Admin");
    }
}