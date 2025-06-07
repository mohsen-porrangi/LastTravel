using FluentValidation;

namespace WalletPayment.Application.Features.BankAccounts.RemoveBankAccount;

/// <summary>
/// Remove bank account validator
/// </summary>
public class RemoveBankAccountValidator : AbstractValidator<RemoveBankAccountCommand>
{
    public RemoveBankAccountValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("شناسه کاربر الزامی است");

        RuleFor(x => x.BankAccountId)
            .NotEmpty()
            .WithMessage("شناسه حساب بانکی الزامی است");
    }
}