using BuildingBlocks.CQRS;
using WalletPayment.Application.Common.Interfaces;
using WalletPayment.Domain.Common;
using WalletPayment.Domain.Common.Contracts;
using WalletPayment.Domain.Exceptions;
using WalletPayment.Domain.ValueObjects;

namespace WalletPayment.Application.Features.Transactions.DirectDeposit;
/// <summary>
/// Direct deposit handler
/// </summary>
public class DirectDepositHandler : ICommandHandler<DirectDepositCommand, DirectDepositResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGatewayClient _paymentGateway;

    public DirectDepositHandler(
        IUnitOfWork unitOfWork,
        IPaymentGatewayClient paymentGateway)
    {
        _unitOfWork = unitOfWork;
        _paymentGateway = paymentGateway;
    }

    public async Task<DirectDepositResult> Handle(DirectDepositCommand request, CancellationToken cancellationToken)
    {
        // Get or create wallet
        var wallet = await _unitOfWork.Wallets.GetByUserIdAsync(request.UserId, cancellationToken);
        if (wallet == null)
        {
            throw new WalletNotFoundException(request.UserId);
        }

        if (!wallet.IsActive)
        {
            return new DirectDepositResult
            {
                IsSuccessful = false,
                ErrorMessage = DomainErrors.GetMessage(DomainErrors.Wallet.Inactive)
            };
        }

        // Get or create currency account
        var account = wallet.GetCurrencyAccount(request.Currency);
        if (account == null)
        {
            account = wallet.CreateCurrencyAccount(request.Currency);
        }

        var money = Money.Create(request.Amount, request.Currency);

        // Create pending transaction
        var transaction = account.CreateDepositTransaction(
            money,
            request.Description);

        // Save pending transaction
        await _unitOfWork.Transactions.AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create payment request
        var paymentResult = await _paymentGateway.CreatePaymentAsync(
            money,
            request.Description,
            request.CallbackUrl,
            orderId: transaction.Id.ToString(),
            cancellationToken: cancellationToken);

        if (!paymentResult.IsSuccessful)
        {
            // Mark transaction as failed
            transaction.MarkAsFailed(paymentResult.ErrorMessage);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new DirectDepositResult
            {
                IsSuccessful = false,
                ErrorMessage = paymentResult.ErrorMessage ?? "خطا در ایجاد درخواست پرداخت"
            };
        }

        // Update transaction with payment authority
        transaction.SetPaymentReference(paymentResult.Authority!);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DirectDepositResult
        {
            IsSuccessful = true,
            PaymentUrl = paymentResult.PaymentUrl,
            Authority = paymentResult.Authority,
            PendingTransactionId = transaction.Id
        };
    }
}