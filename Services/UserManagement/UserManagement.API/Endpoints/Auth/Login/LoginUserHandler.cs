using BuildingBlocks.Exceptions;
using UserManagement.API.Endpoints.Auth.Login;

internal sealed class LoginUserCommandHandler(
    IUnitOfWork uow,
    ITokenService tokenService,
    IOtpService otpService
) : ICommandHandler<LoginUserCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        // حالت 1: فقط شماره موبایل ارسال شده - بررسی وجود کاربر
        if (!string.IsNullOrWhiteSpace(request.Mobile) &&
            string.IsNullOrWhiteSpace(request.Password) &&
            string.IsNullOrWhiteSpace(request.Otp))
        {
            var exists = await uow.Users.UserExistsByMobileAsync(request.Mobile);

            if (exists)
            {
                // کاربر وجود دارد، OTP ارسال می‌کنیم
                await otpService.SendOtpAsync(request.Mobile);
                return new LoginResult(
                    Success: false,
                    Message: "کد تأیید به شماره موبایل شما ارسال شد",
                    NextStep: "enter-otp"
                );
            }
            else
            {
                // کاربر وجود ندارد، به صفحه ثبت‌نام هدایت می‌کنیم
                return new LoginResult(
                    Success: false,
                    Message: "شما هنوز ثبت‌ نام نکرده‌اید",
                    NextStep: "register"
                );
            }
        }

        // حالت 2: ورود با موبایل و رمز عبور
        if (!string.IsNullOrWhiteSpace(request.Mobile) && !string.IsNullOrWhiteSpace(request.Password))
        {
            MasterIdentity? identity = await uow.Users.GetIdentityByMobileAsync(request.Mobile);
            if (identity is null || !BCrypt.Net.BCrypt.Verify(request.Password, identity.PasswordHash))
                throw new UnauthorizedDomainException("اطلاعات ورود نامعتبر است");

            var user = await uow.Users.GetUserByIdentityIdAsync(identity.Id)
                ?? throw new NotFoundException("کاربر یافت نشد", $"کاربری با شناسه هویت {identity.Id} یافت نشد");

            if (!user.MasterIdentity.IsActive)
                throw new ForbiddenDomainException("حساب کاربری غیرفعال است");

            var permissions = await uow.Users.GetUserPermissionsAsync(user.Id);
            var token = tokenService.GenerateToken(user, permissions);

            return new LoginResult(
                Success: true,
                Token: token
            );
        }

        // حالت 3: ورود با موبایل و OTP
        else if (!string.IsNullOrWhiteSpace(request.Mobile) && !string.IsNullOrWhiteSpace(request.Otp))
        {
            await otpService.SendOtpAsync(request.Mobile);
            var isValid = await otpService.ValidateOtpAsync(request.Mobile, request.Otp);
            if (!isValid)
                throw new UnauthorizedDomainException("کد اعتبارسنجی نامعتبر است");

            var identity = await uow.Users.GetIdentityByMobileAsync(request.Mobile)
                ?? throw new NotFoundException("کاربر یافت نشد", $"کاربری با شماره موبایل {request.Mobile} یافت نشد");

            var user = await uow.Users.GetUserByIdentityIdAsync(identity.Id)
                ?? throw new NotFoundException("کاربر یافت نشد", $"کاربری با شناسه هویت {identity.Id} یافت نشد");

            if (!user.MasterIdentity.IsActive)
                throw new ForbiddenDomainException("حساب کاربری غیرفعال است");

            var permissions = await uow.Users.GetUserPermissionsAsync(user.Id);
            var token = tokenService.GenerateToken(user, permissions);

            return new LoginResult(
                Success: true,
                Token: token
            );
        }

        // ورودی نامعتبر
        throw new BadRequestException("پارامترهای ورود ناقص هستند",
            "باید شماره موبایل همراه با رمز عبور یا کد OTP ارائه شود");
    }
}