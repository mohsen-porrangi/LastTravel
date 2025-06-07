namespace WalletPayment.Application.Common.Interfaces;

/// <summary>
/// Current user service interface
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Get current user ID
    /// </summary>
    Guid GetCurrentUserId();
    /// <summary>
    /// Get current MasterIdentity ID
    /// </summary>
    Guid GetCurrentMasterIdentityId();
    /// <summary>
    /// Check if user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Get current user's wallet ID (cached)
    /// </summary>
    Task<Guid?> GetCurrentWalletIdAsync(CancellationToken cancellationToken = default);
}