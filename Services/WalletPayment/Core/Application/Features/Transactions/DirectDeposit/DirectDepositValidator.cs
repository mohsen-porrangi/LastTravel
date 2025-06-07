using FluentValidation;
using WalletPayment.Application.Features.Transactions.DirectDeposit;
using WalletPayment.Domain.Common;
using WalletPayment.Domain.Enums;
using WalletPayment.Domain.ValueObjects;

namespace WalletPayment.Application.Features.Transactions.DirectDeposit;

/// <summary>
/// Direct deposit validator
/// </summary>
public class DirectDepositValidator : AbstractValidator<DirectDepositCommand>
{
    public DirectDepositValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("شناسه کاربر الزامی است");

        RuleFor(x => x.Amount)
            .Must(amount => BusinessRules.Amounts.IsValidTransactionAmount(Money.Create(amount, CurrencyCode.IRR)))
            .WithMessage($"مبلغ باید بین {BusinessRules.Amounts.MinimumTransactionAmount.Value:N0} تا {BusinessRules.Amounts.MaximumSingleTransactionAmount.Value:N0} ریال باشد");

        RuleFor(x => x.Currency)
            .Must(BusinessRules.Currency.IsSupportedCurrency)
            .WithMessage("ارز انتخابی پشتیبانی نمی‌شود");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("توضیحات الزامی است")
            .MaximumLength(500)
            .WithMessage("توضیحات نباید بیش از 500 کاراکتر باشد");

        RuleFor(x => x.CallbackUrl)
            .NotEmpty()
            .WithMessage("آدرس بازگشت الزامی است")
            .Must(BeValidUrl)
            .WithMessage("آدرس بازگشت نامعتبر است");
    }

    private static bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}