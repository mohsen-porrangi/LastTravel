/// <summary>
/// سرویس نگهداری موقت اطلاعات ثبت‌نام تا تأیید OTP
/// </summary>
public interface ITemporaryRegistrationService
{
    /// <summary>
    /// ذخیره موقت اطلاعات ثبت‌نام برای 2 دقیقه
    /// </summary>
    Task StoreTemporaryRegistrationAsync(string mobile, string passwordHash);

    /// <summary>
    /// دریافت اطلاعات موقت ثبت‌نام
    /// </summary>
    Task<TemporaryRegistrationData?> GetTemporaryRegistrationAsync(string mobile);

    /// <summary>
    /// حذف اطلاعات موقت بعد از تأیید موفق
    /// </summary>
    Task RemoveTemporaryRegistrationAsync(string mobile);

    /// <summary>
    /// بررسی وجود درخواست ثبت‌نام موقت
    /// </summary>
    Task<bool> HasPendingRegistrationAsync(string mobile);
}

/// <summary>
/// مدل داده‌های موقت ثبت‌نام
/// </summary>
public record TemporaryRegistrationData(
    string Mobile,
    string PasswordHash,
    DateTime CreatedAt
);