namespace UserManagement.API.Endpoints.Auth.Login;

public record LoginUserCommand(
    //string? Email,
    string? Mobile,
    string? Password,
    string? Otp
) : ICommand<LoginResult>;

public record LoginResult(
    bool Success,
    string? Token = null,
    string? Message = null,
    string? NextStep = null
);