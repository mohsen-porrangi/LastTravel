namespace UserManagement.API.Endpoints.Auth.Password.ResetPassword;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Mobile)
            .NotEmpty().WithMessage("شماره موبایل الزامی است.")
            .Matches(@"(\+98|0)?9\d{9}$").WithMessage("فرمت شماره موبایل معتبر نیست.");

        When(x => !string.IsNullOrEmpty(x.Otp) || !string.IsNullOrEmpty(x.NewPassword), () =>
        {
            RuleFor(x => x.Otp)
                .NotEmpty().WithMessage("کد تأیید الزامی است.")
                .Matches(@"^\d{6}$").WithMessage("کد تأیید باید ۶ رقم باشد.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("رمز عبور جدید الزامی است.")
                .MinimumLength(6).WithMessage("رمز عبور جدید باید حداقل ۶ کاراکتر باشد.");
        });
    }
}