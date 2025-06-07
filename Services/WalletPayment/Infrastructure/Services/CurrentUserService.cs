using BuildingBlocks.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using WalletPayment.Application.Common.Interfaces;
using WalletPayment.Domain.Common.Contracts;

namespace WalletPayment.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork) : ICurrentUserService
{
    public Guid GetCurrentUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User
           .FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            return userId;

        throw new UnauthorizedDomainException("کاربر احراز هویت نشده است");
    }
    public Guid GetCurrentMasterIdentityId()
    {
        var masterIdClaim = httpContextAccessor.HttpContext?.User
            .FindFirst("MasterIdentityId");

        if (masterIdClaim != null && Guid.TryParse(masterIdClaim.Value, out var masterId))
            return masterId;

        throw new UnauthorizedDomainException("شناسه اصلی کاربر یافت نشد");
    }

    public Guid GetCurrentUserAccountId()
    {
        var accountIdClaim = httpContextAccessor.HttpContext?.User
          .FindFirst("AccountId");

        if (accountIdClaim != null && Guid.TryParse(accountIdClaim.Value, out var accountId))
            return accountId;

        throw new UnauthorizedDomainException("شناسه حساب کاربری یافت نشد");
    }

    public async Task<Guid?> GetCurrentWalletIdAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated)
            return null;

        var userId = GetCurrentUserId();
        var wallet = await unitOfWork.Wallets.GetByUserIdAsync(userId, cancellationToken);

        return wallet?.Id;
    }
    /// <summary>
    /// Get user roles from JWT token
    /// </summary>
    public IEnumerable<string> GetCurrentUserRoles()
    {
        return httpContextAccessor.HttpContext?.User
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
        return httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Get user mobile from token
    /// </summary>
    public string? GetCurrentUserMobile()
    {
        return httpContextAccessor.HttpContext?.User
            .FindFirst("Mobile")?.Value;
    }
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
}
