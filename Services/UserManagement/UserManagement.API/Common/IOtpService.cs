namespace UserManagement.API.Common;

public interface IOtpService
{
    Task SendOtpAsync(string mobile);
    Task<bool> ValidateOtpAsync(string mobile, string otp);
}
