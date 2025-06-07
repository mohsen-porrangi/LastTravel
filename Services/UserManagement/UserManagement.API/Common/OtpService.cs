namespace UserManagement.API.Common;

public class OtpService : IOtpService
{
    // به عنوان یک نمونه ساده موقت
    private static readonly Dictionary<string, string> OtpStore = new();

    public Task SendOtpAsync(string mobile)
    {

        var simulatedOtp = "111111";
        OtpStore[mobile] = simulatedOtp;
        Console.WriteLine($"[OTP TEST MODE] Sending OTP {simulatedOtp} to {mobile}");
        return Task.CompletedTask;


        // TODO: Replace this stub with actual OTP sending logic

        var otp = new Random().Next(100000, 999999).ToString();


        OtpStore[mobile] = otp;

        // TODO 
        Console.WriteLine($"[OTP Service] OTP for {mobile}: {otp}");

        return Task.CompletedTask;
    }

    public Task<bool> ValidateOtpAsync(string mobile, string otp)
    {
        var isValid = OtpStore.TryGetValue(mobile, out var storedOtp) && storedOtp == otp;

        if (isValid)
            OtpStore.Remove(mobile);

        return Task.FromResult(isValid);
    }
}
