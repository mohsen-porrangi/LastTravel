using BuildingBlocks.Contracts;

namespace UserManagement.API.Common;

public interface IUserRepository : IRepositoryBase<User, Guid>
{
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByMobileAsync(string mobile);

    Task CreateAsync(User user, MasterIdentity identity);
    Task UpdateIdentityAsync(MasterIdentity identity);

    Task<MasterIdentity?> GetIdentityByEmailAsync(string email);
    Task<MasterIdentity?> GetIdentityByMobileAsync(string mobile);
    Task<MasterIdentity?> GetIdentityByUsernameAsync(string username);
    Task<User?> GetUserByIdentityIdAsync(Guid identityId);

    Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId);
    Task AddLoginHistoryAsync(LoginHistory history);

    // Reset password flow:
    Task StorePasswordResetTokenAsync(Guid identityId, Guid resetToken);
    Task<MasterIdentity?> GetIdentityByResetTokenAsync(Guid resetToken);
    Task RemoveResetTokenAsync(Guid resetToken);
    Task<bool> MobileExistsAsync(string mobile, CancellationToken cancellationToken = default);
    Task AddIdentityAsync(MasterIdentity identity, CancellationToken cancellationToken = default);
    Task<MasterIdentity?> GetIdentityByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MasterIdentity?> GetIdentityByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task StoreRefreshTokenAsync(Guid identityId, string refreshToken, CancellationToken cancellationToken = default);
    Task InvalidateRefreshTokenAsync(Guid identityId, CancellationToken cancellationToken = default);
    Task<bool> UserExistsByMobileAsync(string mobile);

}
