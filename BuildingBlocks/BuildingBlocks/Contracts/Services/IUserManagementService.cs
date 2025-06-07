namespace BuildingBlocks.Contracts.Services
{
    /// <summary>
    /// رابط سرویس مدیریت کاربر برای استفاده توسط سایر سرویس‌ها
    /// </summary>
    public interface IUserManagementService
    {
        Task<bool> UserExistsAsync(Guid userId);
        Task<bool> IsUserActiveAsync(Guid userId);

        // عملیات احراز هویت - محدود به آنچه سایر سرویس‌ها نیاز دارند
        Task<bool> ValidateCredentialsAsync(string mobile, string password);
        Task<TokenResponseDto> AuthenticateAsync(AuthRequestDto request);
        Task<bool> ValidateTokenAsync(string token);

        // عملیات مجوزها - مورد نیاز برای بررسی دسترسی بین سرویس‌ها
        Task<bool> HasPermissionAsync(Guid userId, string permissionCode);
        Task<UserDetailDto> GetUserByIdAsync(Guid userId);
    }

    // DTOهای مرتبط به صورت record برای وحدت رویه با کد فعلی


    public record AuthRequestDto(
        string Mobile,
        string Password,
        string Otp
    );

    public record TokenResponseDto(
        string Token,
        string RefreshToken,
        DateTime ExpiresAt
    );
    public record UserDetailDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Mobile,
    bool IsActive
);
}