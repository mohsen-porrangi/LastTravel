namespace UserManagement.API.Endpoints.Admin.RoleManagement.UnassignPermission;

public record UnassignPermissionCommand(int RoleId, int PermissionId) : ICommand;
