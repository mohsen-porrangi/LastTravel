namespace UserManagement.API.Endpoints.Admin.RoleManagement.AssignRole;

public record AssignRoleToUserCommand(Guid UserId, int RoleId) : ICommand;