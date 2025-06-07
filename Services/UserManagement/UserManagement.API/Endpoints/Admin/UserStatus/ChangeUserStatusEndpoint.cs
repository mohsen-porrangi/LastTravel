namespace UserManagement.API.Endpoints.Admin.UserStatus;

public class ChangeUserStatusEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/users/{id:guid}/status", async (
            Guid id,
            ChangeUserStatusRequest body,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new ChangeUserStatusCommand(id, body.IsActive);
            await sender.Send(command, ct);
            return Results.NoContent();
        })
        .WithName("ToggleUserStatus")
        .WithDescription("تغییر وضعیت فعال/غیرفعال کاربر")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithTags("Users")
        .RequireAuthorization("Admin"); 
    }
}


