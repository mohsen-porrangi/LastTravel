namespace UserManagement.API.Endpoints.Admin.RoleManagement.AssignPermission;

public record AssignPermissionCommand(int RoleId, int PermissionId) : ICommand;
