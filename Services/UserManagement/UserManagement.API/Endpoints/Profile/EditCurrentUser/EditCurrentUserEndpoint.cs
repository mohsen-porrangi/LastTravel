using System.Security.Claims;
using UserManagement.API.Common.Extensions;

namespace UserManagement.API.Endpoints.Profile.EditCurrentUser
{
    public class EditCurrentUserEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/users/current", async (
                EditCurrentUserCommand command,
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken ct) =>
            {
                var identityId = user.GetIdentityId();
                var fullCommand = command with { UserId = identityId };

                await sender.Send(fullCommand, ct);
                return Results.NoContent();
            })
                .WithName("EditUserProfile")
                .WithDescription("ویرایش اطلاعات پروفایل کاربر و تغییر رمز عبور")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithTags("Profile")
                .RequireAuthorization();
        }
    }
}