using BuildingBlocks.CQRS;
using WalletPayment.Domain.Common.Contracts;

namespace WalletPayment.Application.Features.BankAccounts.RemoveBankAccount;

/// <summary>
/// Remove bank account handler
/// </summary>
public class RemoveBankAccountHandler : ICommandHandler<RemoveBankAccountCommand, RemoveBankAccountResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoveBankAccountHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RemoveBankAccountResult> Handle(RemoveBankAccountCommand request, CancellationToken cancellationToken)
    {
        var wallet = await _unitOfWork.Wallets.GetByUserIdWithIncludesAsync(
            request.UserId,
            includeCurrencyAccounts: false,
            includeBankAccounts: true,
            cancellationToken: cancellationToken);

        if (wallet == null)
        {
            return new RemoveBankAccountResult
            {
                IsSuccessful = false,
                ErrorMessage = "کیف پول یافت نشد",
                BankAccountId = request.BankAccountId
            };
        }

        try
        {
            wallet.RemoveBankAccount(request.BankAccountId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new RemoveBankAccountResult
            {
                IsSuccessful = true,
                BankAccountId = request.BankAccountId,
                RemovedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new RemoveBankAccountResult
            {
                IsSuccessful = false,
                ErrorMessage = ex.Message,
                BankAccountId = request.BankAccountId
            };
        }
    }
}