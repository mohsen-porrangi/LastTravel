using BuildingBlocks.CQRS;
using WalletPayment.Application.Common.Interfaces;
using WalletPayment.Domain.Common.Contracts;
using WalletPayment.Domain.Enums;
using WalletPayment.Domain.Exceptions;

namespace WalletPayment.Application.Features.Transactions.ProcessPaymentCallback;

/// <summary>
/// Payment callback handler
/// </summary>
public class ProcessPaymentCallbackHandler : ICommandHandler<ProcessPaymentCallbackCommand, PaymentCallbackResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGatewayClient _paymentGateway;

    public ProcessPaymentCallbackHandler(
        IUnitOfWork unitOfWork,
        IPaymentGatewayClient paymentGateway)
    {
        _unitOfWork = unitOfWork;
        _paymentGateway = paymentGateway;
    }

    public async Task<PaymentCallbackResult> Handle(ProcessPaymentCallbackCommand request, CancellationToken cancellationToken)
    {
        // Find transaction by payment authority
        var transaction = await _unitOfWork.Transactions.GetByPaymentReferenceAsync(
            request.Authority, cancellationToken);

        if (transaction == null)
        {
            return new PaymentCallbackResult
            {
                IsSuccessful = false,
                ErrorMessage = "تراکنش مرتبط با این پرداخت یافت نشد"
            };
        }

        // Check if already processed
        if (transaction.Status == TransactionStatus.Completed)
        {
            // Get wallet for balance info
            var wallet = await _unitOfWork.Wallets.GetByIdAsync(transaction.WalletId, cancellationToken: cancellationToken);
            var account = wallet?.GetCurrencyAccount(transaction.Amount.Currency);

            return new PaymentCallbackResult
            {
                IsSuccessful = true,
                IsVerified = true,
                TransactionId = transaction.Id,
                WalletId = transaction.WalletId,
                Amount = transaction.Amount.Value,
                Currency = transaction.Amount.Currency,
                NewBalance = account?.Balance.Value,
                ReferenceId = transaction.PaymentReferenceId,
                ProcessedAt = transaction.ProcessedAt
            };
        }

        // Handle failed payment
        if (request.Status != "OK")
        {
            transaction.MarkAsFailed($"Payment failed with status: {request.Status}");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new PaymentCallbackResult
            {
                IsSuccessful = false,
                TransactionId = transaction.Id,
                WalletId = transaction.WalletId,
                ErrorMessage = "پرداخت ناموفق بود"
            };
        }

        // Verify payment with gateway
        var verificationResult = await _paymentGateway.VerifyPaymentAsync(
            request.Authority,
            transaction.Amount,
            request.Gateway,
            cancellationToken);

        if (!verificationResult.IsSuccessful || !verificationResult.IsVerified)
        {
            transaction.MarkAsFailed(verificationResult.ErrorMessage ?? "Payment verification failed");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new PaymentCallbackResult
            {
                IsSuccessful = false,
                TransactionId = transaction.Id,
                WalletId = transaction.WalletId,
                ErrorMessage = verificationResult.ErrorMessage ?? "تأیید پرداخت ناموفق بود"
            };
        }

        // Validate amount
        if (request.Amount.HasValue && Math.Abs(transaction.Amount.Value - request.Amount.Value) > 0.01m)
        {
            transaction.MarkAsFailed($"Amount mismatch: expected {transaction.Amount.Value}, received {request.Amount.Value}");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new PaymentCallbackResult
            {
                IsSuccessful = false,
                TransactionId = transaction.Id,
                ErrorMessage = "مبلغ پرداخت با مبلغ درخواستی مطابقت ندارد"
            };
        }

        // Execute transaction in database transaction
        return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            // Get wallet with currency account
            var walletWithAccount = await _unitOfWork.Wallets.GetByUserIdWithIncludesAsync(
                transaction.UserId,
                includeCurrencyAccounts: true,
                cancellationToken: ct);

            if (walletWithAccount == null)
            {
                throw new WalletNotFoundException(transaction.UserId);
            }

            var account = walletWithAccount.GetCurrencyAccount(transaction.Amount.Currency);
            if (account == null)
            {
                throw new InvalidCurrencyAccountException(transaction.Amount.Currency);
            }

            // Process the deposit
            account.ProcessDeposit(transaction);

            // Mark transaction as completed
            transaction.MarkAsCompleted();

            // Set verification reference
            if (!string.IsNullOrEmpty(verificationResult.ReferenceId))
            {
                transaction.SetPaymentReference(verificationResult.ReferenceId);
            }

            // Save changes
            await _unitOfWork.SaveChangesAsync(ct);

            return new PaymentCallbackResult
            {
                IsSuccessful = true,
                IsVerified = true,
                TransactionId = transaction.Id,
                WalletId = transaction.WalletId,
                Amount = transaction.Amount.Value,
                Currency = transaction.Amount.Currency,
                NewBalance = account.Balance.Value,
                ReferenceId = verificationResult.ReferenceId,
                ProcessedAt = DateTime.UtcNow
            };
        }, cancellationToken);
    }
}