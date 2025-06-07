namespace BuildingBlocks.Contracts;
public interface ICurrentUserService
{
    Guid GetCurrentUserId();
    Guid GetCurrentUserAccountId();
    Guid GetCurrentMasterIdentityId();
    bool IsAuthenticated { get; }
}
