// Application/Features/Transactions/IntegratedPurchase/IntegratedPurchaseValidator.cs
using FluentValidation;
using WalletPayment.Application.Common.Validation;

namespace WalletPayment.Application.Features.Transactions.IntegratedPurchase;

/// <summary>
/// Integrated purchase validator - Fixed Implementation
/// </summary>
public class IntegratedPurchaseValidator : AbstractValidator<IntegratedPurchaseCommand>
{
    public IntegratedPurchaseValidator()
    {
        RuleFor(x => x.UserId)
            .ValidateUserId();

        RuleFor(x => x.TotalAmount)
            .ValidateTransactionAmount();

        RuleFor(x => x.Currency)
            .ValidateSupportedCurrency();

        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("شناسه سفارش الزامی است")
            .MaximumLength(50).WithMessage("شناسه سفارش نباید بیش از 50 کاراکتر باشد");

        RuleFor(x => x.Description)
            .ValidateTransactionDescription();

        When(x => !x.UseCredit, () =>
        {
            RuleFor(x => x.CallbackUrl)
                .ValidateCallbackUrl();
        });
    }
}