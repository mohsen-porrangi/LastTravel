namespace UserManagement.API.Endpoints.Admin.UserStatus;

public record ChangeUserStatusCommand(Guid Id, bool IsActive) : ICommand;
public record ChangeUserStatusRequest(bool IsActive);