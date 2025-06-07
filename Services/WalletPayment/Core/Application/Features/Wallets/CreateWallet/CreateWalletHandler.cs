using BuildingBlocks.CQRS;
using WalletPayment.Application.Features.Wallets.CreateWallet;
using WalletPayment.Domain.Aggregates.WalletAggregate;
using WalletPayment.Domain.Common.Contracts;
using WalletPayment.Domain.Enums;

namespace WalletPayment.Application.Features.Wallets.CreateWallet;

public class CreateWalletHandler : ICommandHandler<CreateWalletCommand, CreateWalletResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateWalletHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateWalletResult> Handle(CreateWalletCommand request, CancellationToken cancellationToken)
    {
        // Check if user already has a wallet
        var existingWallet = await _unitOfWork.Wallets.GetByUserIdAsync(request.UserId, cancellationToken);
        if (existingWallet != null)
        {
            // Return existing wallet info instead of throwing exception
            var existingAccount = existingWallet.GetCurrencyAccount(CurrencyCode.IRR);
            return new CreateWalletResult
            {
                WalletId = existingWallet.Id,
                UserId = existingWallet.UserId,
                DefaultAccountId = existingAccount?.Id,
                DefaultCurrency = CurrencyCode.IRR,
                CreatedAt = existingWallet.CreatedAt
            };
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            // Create new wallet
            var wallet = new Wallet(request.UserId);

            // Create default IRR currency account if requested
            CurrencyAccount? defaultAccount = null;
            if (request.CreateDefaultAccount)
            {
                defaultAccount = wallet.CreateCurrencyAccount(CurrencyCode.IRR);
            }

            // Save wallet (includes currency accounts via navigation properties)
            await _unitOfWork.Wallets.AddAsync(wallet, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return new CreateWalletResult
            {
                WalletId = wallet.Id,
                UserId = wallet.UserId,
                DefaultAccountId = defaultAccount?.Id,
                DefaultCurrency = CurrencyCode.IRR,
                CreatedAt = wallet.CreatedAt
            };
        }, cancellationToken);
    }
}