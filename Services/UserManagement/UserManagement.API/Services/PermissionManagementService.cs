using Microsoft.Extensions.Caching.Memory;
using UserManagement.API.Contracts;
using UserManagement.API.Infrastructure.Data;

namespace UserManagement.API.Services;

//  Primary Constructor
public class PermissionManagementService(
    UserDbContext db,
    IMemoryCache cache,
    ILogger<PermissionManagementService> logger
) : IPermissionManagementService
{
    private const string PERMISSIONS_CACHE_KEY = "all_permissions";
    private const string PERMISSION_BY_CODE_PREFIX = "permission_code_";
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromHours(1);

    public async Task<IReadOnlyList<PermissionDto>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        //  Pattern matching with when clause
        if (cache.TryGetValue(PERMISSIONS_CACHE_KEY, out var cachedValue) && cachedValue is IReadOnlyList<PermissionDto> cachedPermissions)
        {
            logger.LogDebug("Retrieved {Count} permissions from cache", cachedPermissions.Count);
            return cachedPermissions;
        }

        //  Collection expression (compatible with EF Core)
        var permissions = await db.Permissions
            .Select(p => new PermissionDto(
                p.Id,
                p.Module,
                p.Action,
                p.Code,
                p.Description
            ))
            .ToListAsync(cancellationToken);

        //  Convert to readonly collection
        IReadOnlyList<PermissionDto> readOnlyPermissions = permissions.AsReadOnly();

        // ذخیره در cache
        cache.Set(PERMISSIONS_CACHE_KEY, readOnlyPermissions, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheExpiry,
            Size = readOnlyPermissions.Count
        });

        logger.LogInformation("Loaded {Count} permissions from database into cache", permissions.Count);
        return readOnlyPermissions;
    }

    public async Task<PermissionDto?> GetPermissionByCodeAsync(string module, string action, CancellationToken cancellationToken = default)
    {
        var code = $"{module}.{action}";
        var cacheKey = $"{PERMISSION_BY_CODE_PREFIX}{code}";

        //  Pattern matching with null check
        if (cache.TryGetValue(cacheKey, out var cachedValue) && cachedValue is PermissionDto cachedPermission)
        {
            return cachedPermission;
        }

        //  Combined LINQ operations
        var permission = await db.Permissions
            .Where(p => p.Module == module && p.Action == action)
            .Select(p => new PermissionDto(
                p.Id,
                p.Module,
                p.Action,
                p.Code,
                p.Description
            ))
            .FirstOrDefaultAsync(cancellationToken);

        //  حالا permission میتونه null باشه چون reference type هست
        if (permission is not null)
        {
            cache.Set(cacheKey, permission, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheExpiry,
                Size = 1
            });
            logger.LogDebug("Permission {Code} cached", code);
        }

        return permission; //  میتونه null برگردونه
    }

    public async Task<bool> PermissionExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        //  Pattern matching with destructuring
        var parts = code.Split('.');
        if (parts is not [var module, var action])
        {
            logger.LogWarning("Invalid permission code format: {Code}", code);
            return false;
        }

        var permission = await GetPermissionByCodeAsync(module, action, cancellationToken);
        return permission is not null;
    }

    public Task RefreshPermissionCacheAsync(CancellationToken cancellationToken = default)
    {
        //  Simple approach - remove main cache key
        cache.Remove(PERMISSIONS_CACHE_KEY);

        logger.LogInformation("Permission cache refreshed");
        return Task.CompletedTask;
    }
}