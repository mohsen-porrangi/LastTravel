using BuildingBlocks.CQRS;
using WalletPayment.Domain.Common.Contracts;
using WalletPayment.Domain.Exceptions;

namespace WalletPayment.Application.Features.BankAccounts.GetBankAccounts;

/// <summary>
/// Get bank accounts handler
/// </summary>
public class GetBankAccountsHandler : IQueryHandler<GetBankAccountsQuery, GetBankAccountsResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBankAccountsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetBankAccountsResult> Handle(GetBankAccountsQuery request, CancellationToken cancellationToken)
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

        var bankAccountDtos = wallet.BankAccounts
            .Where(ba => !ba.IsDeleted)
            .OrderByDescending(ba => ba.IsDefault)
            .ThenByDescending(ba => ba.CreatedAt)
            .Select(ba => new BankAccountDto
            {
                Id = ba.Id,
                BankName = ba.BankName,
                MaskedAccountNumber = ba.GetMaskedAccountNumber(),
                MaskedCardNumber = ba.GetMaskedCardNumber(),
                ShabaNumber = ba.ShabaNumber,
                AccountHolderName = ba.AccountHolderName,
                IsVerified = ba.IsVerified,
                IsDefault = ba.IsDefault,
                IsActive = ba.IsActive,
                CreatedAt = ba.CreatedAt
            });

        return new GetBankAccountsResult
        {
            BankAccounts = bankAccountDtos
        };
    }
}
