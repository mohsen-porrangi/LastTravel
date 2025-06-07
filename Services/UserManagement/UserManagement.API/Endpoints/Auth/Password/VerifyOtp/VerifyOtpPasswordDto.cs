namespace UserManagement.API.Endpoints.Auth.Password.VerifyOtp;


public record VerifyResetPasswordOtpCommand(Guid ResetToken, string Otp) : ICommand<bool>;
