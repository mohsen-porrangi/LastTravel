using BuildingBlocks.Contracts.Security;
using System.Security.Claims;

namespace UserManagement.API.Infrastructure.Middleware;

//  Primary Constructor در Web layer
public sealed class PermissionMiddleware(
    RequestDelegate next,
    IServiceScopeFactory scopeFactory,
    ILogger<PermissionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        using var scope = scopeFactory.CreateScope();
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

        var endpoint = context.GetEndpoint();
        var permissionMetadata = endpoint?.Metadata?.GetOrderedMetadata<RequirePermissionAttribute>();

        if (permissionMetadata == null || !permissionMetadata.Any())
        {
            await next(context);
            return;
        }

        //  Convert to array for better performance
        var permissionAttributes = permissionMetadata.ToArray();

        //  Pattern matching with property patterns
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value switch
        {
            { } userIdStr when Guid.TryParse(userIdStr, out var id) => id,
            _ => null as Guid?
        };

        if (userId is null)
        {
            logger.LogWarning("Unauthorized access attempt - no valid user ID in claims");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            await context.Response.WriteAsJsonAsync(new { error = "دسترسی غیرمجاز: احراز هویت نشده" });
            return;
        }

        logger.LogDebug("Checking permissions for user {UserId}", userId);

        var userPermissions = await permissionService.GetUserPermissionsAsync(userId.Value);
        var userPermissionSet = userPermissions.ToHashSet(); // O(1) lookup

        //  LINQ improvements
        var missingPermissions = permissionAttributes
            .Select(attr => attr.Permission)
            .Where(permission => !userPermissionSet.Contains(permission))
            .ToArray();

        if (missingPermissions.Length > 0)
        {
            logger.LogWarning("Access denied for user {UserId} - missing permissions: {Permissions}",
                userId, string.Join(", ", missingPermissions));

            context.Response.StatusCode = StatusCodes.Status403Forbidden;

            await context.Response.WriteAsJsonAsync(new
            {
                error = "دسترسی مجاز نیست",
                missingPermissions = missingPermissions
            });
            return;
        }

        logger.LogDebug("All permission checks passed for user {UserId}", userId);
        await next(context);
    }
}