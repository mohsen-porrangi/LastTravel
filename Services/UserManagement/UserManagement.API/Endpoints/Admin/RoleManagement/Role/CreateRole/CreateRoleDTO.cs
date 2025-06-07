namespace UserManagement.API.Endpoints.Admin.RoleManagement.Role.CreateRole;

public record CreateRoleCommand(string Name) : ICommand<int>;
