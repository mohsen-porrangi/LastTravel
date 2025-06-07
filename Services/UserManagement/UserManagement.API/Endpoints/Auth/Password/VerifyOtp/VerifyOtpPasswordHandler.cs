namespace UserManagement.API.Endpoints.Auth.Password.VerifyOtp;

internal sealed class VerifyResetPasswordOtpCommandHandler(
    IUnitOfWork uow,
    IOtpService otpService
) : ICommandHandler<VerifyResetPasswordOtpCommand, bool>
{
    public async Task<bool> Handle(VerifyResetPasswordOtpCommand command, CancellationToken cancellationToken)
    {
        var identity = await uow.Users.GetIdentityByResetTokenAsync(command.ResetToken)
                       ?? throw new InvalidOperationException("توکن معتبر نیست");

        return await otpService.ValidateOtpAsync(identity.Mobile, command.Otp);
    }
}