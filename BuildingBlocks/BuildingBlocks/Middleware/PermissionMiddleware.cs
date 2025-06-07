using BuildingBlocks.Contracts.Security;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BuildingBlocks.Middleware;

public class PermissionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IPermissionService _permissionService;

    public PermissionMiddleware(RequestDelegate next, IPermissionService permissionService)
    {
        _next = next;
        _permissionService = permissionService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requiredPermission = context.Items["RequiredPermission"] as string;

        if (!string.IsNullOrEmpty(requiredPermission))
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var permissions = await _permissionService.GetUserPermissionsAsync(userId);

            if (!permissions.Contains(requiredPermission))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Access Denied");
                return;
            }
        }

        await _next(context);
    }
}