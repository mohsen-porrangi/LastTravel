namespace UserManagement.API.Endpoints.Admin.DeleteUser
{
    public record DeleteUserCommand(Guid Id) : ICommand;
}
