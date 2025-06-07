using BuildingBlocks.Contracts;
using System.Security.Claims;

namespace WalletPayment.API.Middleware;

/// <summary>
/// Middleware to set current user context
/// </summary>
public class CurrentUserMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract user information from JWT token
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                // Add user ID to HttpContext.Items for easy access
                context.Items["UserId"] = userIdClaim.Value;
            }
        }

        await _next(context);
    }
}
