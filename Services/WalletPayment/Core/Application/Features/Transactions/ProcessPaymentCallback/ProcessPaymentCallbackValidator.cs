using FluentValidation;


namespace WalletPayment.Application.Features.Transactions.ProcessPaymentCallback;
/// <summary>
/// Payment callback validator
/// </summary>
public class ProcessPaymentCallbackValidator : AbstractValidator<ProcessPaymentCallbackCommand>
{
    public ProcessPaymentCallbackValidator()
    {
        RuleFor(x => x.Authority)
            .NotEmpty()
            .WithMessage("شناسه مرجع پرداخت الزامی است")
            .Length(36)
            .WithMessage("شناسه مرجع پرداخت نامعتبر است");

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("وضعیت پرداخت الزامی است");

        When(x => x.Status == "OK", () =>
        {
            RuleFor(x => x.Amount)
                .NotNull()
                .WithMessage("مبلغ برای پرداخت موفق الزامی است")
                .GreaterThan(0)
                .WithMessage("مبلغ باید بزرگتر از صفر باشد");
        });
    }
}