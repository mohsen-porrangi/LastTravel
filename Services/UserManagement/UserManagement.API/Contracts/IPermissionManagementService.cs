namespace UserManagement.API.Contracts;
/// <summary>
/// سرویس مدیریت دینامیک Permission ها از دیتابیس
/// </summary>
public interface IPermissionManagementService
{
    /// <summary>
    /// دریافت تمام Permission ها از دیتابیس
    /// </summary>
    Task<IReadOnlyList<PermissionDto>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت Permission بر اساس Module و Action
    /// </summary>
    Task<PermissionDto?> GetPermissionByCodeAsync(string module, string action, CancellationToken cancellationToken = default);

    /// <summary>
    /// بررسی وجود Permission
    /// </summary>
    Task<bool> PermissionExistsAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cache invalidation برای تغییرات Permission ها
    /// </summary>
    Task RefreshPermissionCacheAsync(CancellationToken cancellationToken = default);
}

//  Reference type record برای nullable support
public sealed record PermissionDto(
    int Id,
    string Module,
    string Action,
    string Code,
    string Description
);
