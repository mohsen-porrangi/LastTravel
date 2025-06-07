namespace UserManagement.API.Endpoints.Auth.Password.ResetPassword;

public record ResetPasswordCommand(
    string Mobile,
    string? Otp = null,
    string? NewPassword = null
) : ICommand;