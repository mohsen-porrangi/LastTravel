using FluentValidation;
using WalletPayment.Application.Common.Validation;

namespace WalletPayment.Application.Features.BankAccounts.AddBankAccount;

/// <summary>
/// Add bank account validator
/// ✅ Fixed namespace usage
/// </summary>
public class AddBankAccountValidator : AbstractValidator<AddBankAccountCommand>
{
    public AddBankAccountValidator()
    {
        RuleFor(x => x.UserId)
            .ValidateUserId(); // ✅ از extension method استفاده

        RuleFor(x => x.BankName)
            .ValidateBankName(); // ✅ از extension method استفاده

        RuleFor(x => x.AccountNumber)
            .ValidateBankAccountNumber(); // ✅ از extension method استفاده

        When(x => !string.IsNullOrEmpty(x.CardNumber), () =>
        {
            RuleFor(x => x.CardNumber)
                .ValidateIranianCardNumber(); // ✅ از extension method استفاده
        });

        When(x => !string.IsNullOrEmpty(x.ShabaNumber), () =>
        {
            RuleFor(x => x.ShabaNumber)
                .ValidateIranianShabaNumber(); // ✅ از extension method استفاده
        });

        When(x => !string.IsNullOrEmpty(x.AccountHolderName), () =>
        {
            RuleFor(x => x.AccountHolderName)
                .MaximumLength(200)
                .WithMessage("نام صاحب حساب نباید بیش از 200 کاراکتر باشد");
        });
    }
}
