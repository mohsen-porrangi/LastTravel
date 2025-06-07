using UserManagement.API.Endpoints.Auth.Login;

namespace UserManagement.API.Endpoints.Auth.RegisterUser;

public record VerifyRegisterOtpCommand(string Mobile, string Otp) : ICommand<LoginResult>;
