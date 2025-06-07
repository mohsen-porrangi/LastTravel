namespace UserManagement.API.Endpoints.Admin.RoleManagement.UnassignRole;
public record UnassignRoleFromUserCommand(Guid UserId, int RoleId) : ICommand;

