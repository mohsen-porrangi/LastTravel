using Microsoft.Extensions.Caching.Memory;

namespace UserManagement.API.Common;

/// <summary>
/// پیاده‌سازی سرویس نگهداری موقت با استفاده از Memory Cache
/// </summary>
public class TemporaryRegistrationService(IMemoryCache cache, ILogger<TemporaryRegistrationService> logger)
    : ITemporaryRegistrationService
{
    private static readonly TimeSpan RegistrationExpiry = TimeSpan.FromMinutes(2);
    private const string KeyPrefix = "temp_registration_";

    public Task StoreTemporaryRegistrationAsync(string mobile, string passwordHash)
    {
        var key = GetKey(mobile);
        var data = new TemporaryRegistrationData(mobile, passwordHash, DateTime.UtcNow);

        //cache.Set(key, data, RegistrationExpiry);
        cache.Set(key, data, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = RegistrationExpiry,
            Size = 1
        });

        logger.LogInformation("Temporary registration stored for mobile: {Mobile}", mobile);
        return Task.CompletedTask;
    }

    public Task<TemporaryRegistrationData?> GetTemporaryRegistrationAsync(string mobile)
    {
        var key = GetKey(mobile);
        var data = cache.Get<TemporaryRegistrationData>(key);

        // بررسی انقضا دستی (اضافی برای اطمینان)
        if (data != null && DateTime.UtcNow - data.CreatedAt > RegistrationExpiry)
        {
            cache.Remove(key);
            logger.LogWarning("Expired temporary registration removed for mobile: {Mobile}", mobile);
            return Task.FromResult<TemporaryRegistrationData?>(null);
        }

        return Task.FromResult(data);
    }

    public Task RemoveTemporaryRegistrationAsync(string mobile)
    {
        var key = GetKey(mobile);
        cache.Remove(key);

        logger.LogInformation("Temporary registration removed for mobile: {Mobile}", mobile);
        return Task.CompletedTask;
    }

    public async Task<bool> HasPendingRegistrationAsync(string mobile)
    {
        var data = await GetTemporaryRegistrationAsync(mobile);
        return data != null;
    }

    private static string GetKey(string mobile) => $"{KeyPrefix}{mobile}";
}