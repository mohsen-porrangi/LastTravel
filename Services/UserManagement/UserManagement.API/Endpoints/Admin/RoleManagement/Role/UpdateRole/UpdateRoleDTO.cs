namespace UserManagement.API.Endpoints.Admin.RoleManagement.Role.UpdateRole;

public record UpdateRoleCommand(int Id, string Name) : ICommand;
