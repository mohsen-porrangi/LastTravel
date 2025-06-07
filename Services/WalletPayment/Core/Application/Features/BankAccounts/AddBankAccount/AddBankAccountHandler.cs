using BuildingBlocks.CQRS;
using WalletPayment.Domain.Common.Contracts;
using WalletPayment.Domain.Exceptions;

namespace WalletPayment.Application.Features.BankAccounts.AddBankAccount;

/// <summary>
/// Add bank account handler
/// </summary>
public class AddBankAccountHandler : ICommandHandler<AddBankAccountCommand, AddBankAccountResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddBankAccountHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AddBankAccountResult> Handle(AddBankAccountCommand request, CancellationToken cancellationToken)
    {
        var wallet = await _unitOfWork.Wallets.GetByUserIdWithIncludesAsync(
            request.UserId,
            includeCurrencyAccounts: false,
            includeBankAccounts: true,
            cancellationToken: cancellationToken);

        if (wallet == null)
        {
            throw new WalletNotFoundException(request.UserId);
        }

        var bankAccount = wallet.AddBankAccount(
            request.BankName,
            request.AccountNumber,
            request.CardNumber,
            request.ShabaNumber,
            request.AccountHolderName);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AddBankAccountResult
        {
            BankAccountId = bankAccount.Id,
            BankName = bankAccount.BankName,
            MaskedAccountNumber = bankAccount.GetMaskedAccountNumber(),
            MaskedCardNumber = bankAccount.GetMaskedCardNumber(),
            ShabaNumber = bankAccount.ShabaNumber,
            AccountHolderName = bankAccount.AccountHolderName,
            IsVerified = bankAccount.IsVerified,
            IsDefault = bankAccount.IsDefault,
            IsActive = bankAccount.IsActive,
            CreatedAt = bankAccount.CreatedAt
        };
    }
}