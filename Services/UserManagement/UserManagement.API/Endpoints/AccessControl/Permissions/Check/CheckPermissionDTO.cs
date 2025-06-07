namespace UserManagement.API.Endpoints.AccessControl.Permissions.Check
{
    public record CheckPermissionDTO(Guid UserId, string Permission);

    public record CheckPermissionResponse(bool IsGranted);
}
