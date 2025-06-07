using BuildingBlocks.Exceptions;

namespace UserManagement.API.Endpoints.Auth.Password.ResetPassword;

internal sealed class ResetPasswordCommandHandler(
    IUnitOfWork uow,
    IOtpService otpService
) : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Unit> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        // مرحله 1: فقط شماره موبایل ارسال شده - ارسال OTP
        if (string.IsNullOrEmpty(command.Otp) && string.IsNullOrEmpty(command.NewPassword))
        {
            var identity = await uow.Users.GetIdentityByMobileAsync(command.Mobile)
                ?? throw new InvalidOperationException("کاربری با این شماره وجود ندارد");

            await otpService.SendOtpAsync(command.Mobile);
            var resetToken = Guid.NewGuid();
            await uow.Users.StorePasswordResetTokenAsync(identity.Id, resetToken);
            await uow.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }

        // مرحله 2: شماره موبایل، OTP و رمز جدید ارسال شده - تأیید OTP و تغییر رمز
        if (!string.IsNullOrEmpty(command.Otp) && !string.IsNullOrEmpty(command.NewPassword))
        {
            var identity = await uow.Users.GetIdentityByMobileAsync(command.Mobile)
                ?? throw new InvalidOperationException("کاربری با این شماره وجود ندارد");

            var isOtpValid = await otpService.ValidateOtpAsync(command.Mobile, command.Otp);
            if (!isOtpValid)
                throw new UnauthorizedDomainException("کد تأیید نامعتبر است");

            identity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.NewPassword);
            await uow.Users.UpdateIdentityAsync(identity);
            await uow.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }

        throw new BadRequestException("پارامترهای نامعتبر", "یا فقط شماره موبایل باید ارسال شود یا شماره موبایل به همراه کد تأیید و رمز جدید");
    }
}