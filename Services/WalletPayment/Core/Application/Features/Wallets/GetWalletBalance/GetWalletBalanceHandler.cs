using BuildingBlocks.CQRS;
using WalletPayment.Domain.Common.Contracts;
using WalletPayment.Domain.Exceptions;

namespace WalletPayment.Application.Features.Wallets.GetWalletBalance;

/// <summary>
/// Get wallet balance handler
/// </summary>
public class GetWalletBalanceHandler : IQueryHandler<GetWalletBalanceQuery, WalletBalanceDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetWalletBalanceHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<WalletBalanceDto> Handle(GetWalletBalanceQuery request, CancellationToken cancellationToken)
    {
        var wallet = await _unitOfWork.Wallets.GetByUserIdWithIncludesAsync(
            request.UserId,
            includeCurrencyAccounts: true,
            cancellationToken: cancellationToken);

        if (wallet == null)
        {
            throw new WalletNotFoundException(request.UserId);
        }

        var currencyBalances = wallet.CurrencyAccounts
            .Where(a => a.IsActive && !a.IsDeleted)
            .Select(a => new CurrencyBalanceDto
            {
                Currency = a.Currency,
                Balance = a.Balance.Value,
                IsActive = a.IsActive
            });

        var totalBalanceInIrr = await _unitOfWork.Wallets.GetTotalBalanceInIrrAsync(wallet.Id, cancellationToken);

        return new WalletBalanceDto
        {
            WalletId = wallet.Id,
            UserId = wallet.UserId,
            IsActive = wallet.IsActive,
            TotalBalanceInIrr = totalBalanceInIrr,
            CurrencyBalances = currencyBalances
        };
    }
}
