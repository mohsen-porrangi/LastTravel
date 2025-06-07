using BuildingBlocks.Contracts;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using BuildingBlocks.Exceptions;

namespace WalletPayment.API.Services;

/// <summary>
/// Current user service implementation for API layer
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            return userId;

        throw new UnauthorizedDomainException("کاربر احراز هویت نشده است");
    }

    public Guid GetCurrentUserAccountId()
    {
        var accountIdClaim = _httpContextAccessor.HttpContext?.User
            .FindFirst("AccountId");

        if (accountIdClaim != null && Guid.TryParse(accountIdClaim.Value, out var accountId))
            return accountId;

        throw new UnauthorizedDomainException("شناسه حساب کاربری یافت نشد");
    }

    public Guid GetCurrentMasterIdentityId()
    {
        var masterIdClaim = _httpContextAccessor.HttpContext?.User
            .FindFirst("MasterIdentityId");

        if (masterIdClaim != null && Guid.TryParse(masterIdClaim.Value, out var masterId))
            return masterId;

        throw new UnauthorizedDomainException("شناسه اصلی کاربر یافت نشد");
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    /// <summary>
    /// Get user roles from JWT token
    /// </summary>
    public IEnumerable<string> GetCurrentUserRoles()
    {
        return _httpContextAccessor.HttpContext?.User
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value) ?? [];
    }

    /// <summary>
    /// Check if user has specific role
    /// </summary>
    public bool HasRole(string role)
    {
        return GetCurrentUserRoles().Contains(role);
    }

    /// <summary>
    /// Get user email from token
    /// </summary>
    public string? GetCurrentUserEmail()
    {
        return _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Get user mobile from token
    /// </summary>
    public string? GetCurrentUserMobile()
    {
        return _httpContextAccessor.HttpContext?.User
            .FindFirst("Mobile")?.Value;
    }
}