using System.Security.Claims;
using UserManagement.API.Common.Extensions;

namespace UserManagement.API.Endpoints.Profile.GetCurrentUser
{
    public class GetCurrentUserEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/users/current", async (
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken ct) =>
            {
                var identityId = user.GetIdentityId();
                var result = await sender.Send(new GetCurrentUserQuery(identityId), ct);
                return Results.Ok(result);
            })
                .WithName("GetUserProfile")
                .WithDescription("دریافت اطلاعات پروفایل کاربر جاری")
                .Produces<GetCurrentUserResult>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .WithTags("Profile")
                .RequireAuthorization();
        }
    }
}
