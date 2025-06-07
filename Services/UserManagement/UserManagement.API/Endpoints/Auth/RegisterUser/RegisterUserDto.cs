namespace UserManagement.API.Endpoints.Auth.RegisterUser;

public record RegisterUserCommand(
    //   string Email,
    string Mobile,
    string Password
) : ICommand;