namespace UserManagement.API.Endpoints.Auth.RegisterUser
{
    public class VerifyRegisterOtpCommandValidator : AbstractValidator<VerifyRegisterOtpCommand>
    {
        public VerifyRegisterOtpCommandValidator()
        {
            RuleFor(x => x.Mobile)
                .NotEmpty().WithMessage("شماره موبایل الزامی است.")
                .Matches(@"^09\d{9}$").WithMessage("فرمت شماره موبایل معتبر نیست.");

            RuleFor(x => x.Otp)
                .NotEmpty().WithMessage("کد تأیید الزامی است.")
                .Matches(@"^\d{6}$").WithMessage("کد تأیید باید ۶ رقم باشد.");
        }
    }
}