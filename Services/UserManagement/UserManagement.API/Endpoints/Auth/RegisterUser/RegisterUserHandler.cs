using BuildingBlocks.Exceptions;

namespace UserManagement.API.Endpoints.Auth.RegisterUser;

internal sealed class RegisterUserCommandHandler(
    IUnitOfWork uow,
    IOtpService otpService,
    ITemporaryRegistrationService tempRegistrationService,
    ILogger<RegisterUserCommandHandler> logger
) : ICommandHandler<RegisterUserCommand>
{
    public async Task<Unit> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var mobileExists = await uow.Users.MobileExistsAsync(command.Mobile);
        if (mobileExists)
            throw new BadRequestException("شماره موبایل تکراری است",
                $"کاربری با شماره موبایل {command.Mobile} قبلاً ثبت شده است");

        // بررسی وجود درخواست ثبت‌نام موقت فعال
        var hasPendingRegistration = await tempRegistrationService.HasPendingRegistrationAsync(command.Mobile);
        if (hasPendingRegistration)
        {
            logger.LogInformation("Found pending registration for mobile: {Mobile}, sending new OTP", command.Mobile);

            // اگر درخواست موقت وجود داشت، فقط OTP جدید ارسال می‌کنیم
            await otpService.SendOtpAsync(command.Mobile);
            logger.LogInformation("New OTP sent for pending registration: {Mobile}", command.Mobile);
        }
        try
        {
            // هش کردن رمز عبور
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password);

            // ذخیره موقت اطلاعات ثبت‌نام
            await tempRegistrationService.StoreTemporaryRegistrationAsync(command.Mobile, passwordHash);

            // ارسال OTP
            await otpService.SendOtpAsync(command.Mobile);

            logger.LogInformation("Temporary registration stored and OTP sent for mobile: {Mobile}", command.Mobile);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during registration process for mobile: {Mobile}", command.Mobile);

            // در صورت خطا، اطلاعات موقت را پاک می‌کنیم
            await tempRegistrationService.RemoveTemporaryRegistrationAsync(command.Mobile);

            throw new InternalServerException("خطا در فرآیند ثبت‌نام",
                "لطفاً مجدداً تلاش کنید یا با پشتیبانی تماس بگیرید");
        }
        return Unit.Value;
    }
}
