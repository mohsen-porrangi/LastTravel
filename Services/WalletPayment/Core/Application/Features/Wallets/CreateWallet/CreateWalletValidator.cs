using FluentValidation;
using WalletPayment.Domain.Common;

namespace WalletPayment.Application.Features.Wallets.CreateWallet;

/// <summary>
/// Create wallet command validator
/// </summary>
public class CreateWalletValidator : AbstractValidator<CreateWalletCommand>
{
    public CreateWalletValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("شناسه کاربر الزامی است")
            .WithErrorCode(DomainErrors.Wallet.InvalidUser);
    }
}
