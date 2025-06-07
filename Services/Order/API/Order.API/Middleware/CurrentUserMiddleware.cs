using System.Security.Claims;

namespace Order.API.Middleware;

public class CurrentUserMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                context.Items["UserId"] = Guid.Parse(userId);
            }

            var identityId = context.User.FindFirst("IdentityId")?.Value;
            if (!string.IsNullOrEmpty(identityId))
            {
                context.Items["IdentityId"] = Guid.Parse(identityId);
            }
        }

        await next(context);
    }
}

public static class CurrentUserMiddlewareExtensions
{
    public static IApplicationBuilder UseCurrentUser(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CurrentUserMiddleware>();
    }
}