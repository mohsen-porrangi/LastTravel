namespace BuildingBlocks.Contracts.Security;

public interface IPermissionService
{
    Task<List<string>> GetUserPermissionsAsync(Guid userId);
}
