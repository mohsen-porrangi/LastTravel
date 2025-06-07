using FluentValidation;

namespace WalletPayment.Application.Features.Transactions.RefundTransaction;

/// <summary>
/// Refund transaction validator
/// </summary>
public class RefundTransactionValidator : AbstractValidator<RefundTransactionCommand>
{
    public RefundTransactionValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("شناسه کاربر الزامی است");

        RuleFor(x => x.OriginalTransactionId)
            .NotEmpty()
            .WithMessage("شناسه تراکنش اصلی الزامی است");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("دلیل استرداد الزامی است")
            .MaximumLength(500)
            .WithMessage("دلیل استرداد نباید بیش از 500 کاراکتر باشد");

        When(x => x.PartialAmount.HasValue, () =>
        {
            RuleFor(x => x.PartialAmount!.Value)
                .GreaterThan(0)
                .WithMessage("مبلغ استرداد باید بزرگتر از صفر باشد");
        });
    }
}
