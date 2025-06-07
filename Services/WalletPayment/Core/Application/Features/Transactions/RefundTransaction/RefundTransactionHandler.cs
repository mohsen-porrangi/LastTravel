using BuildingBlocks.CQRS;
using WalletPayment.Domain.Common.Contracts;
using WalletPayment.Domain.Exceptions;
using WalletPayment.Domain.Enums;
using WalletPayment.Domain.ValueObjects;
using WalletPayment.Domain.Aggregates.TransactionAggregate;

namespace WalletPayment.Application.Features.Transactions.RefundTransaction;

/// <summary>
/// Refund transaction handler
/// </summary>
public class RefundTransactionHandler : ICommandHandler<RefundTransactionCommand, RefundTransactionResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public RefundTransactionHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RefundTransactionResult> Handle(RefundTransactionCommand request, CancellationToken cancellationToken)
    {
        // Get original transaction
        var originalTransaction = await _unitOfWork.Transactions.GetByIdAsync(
            request.OriginalTransactionId, track: true, cancellationToken);

        if (originalTransaction == null)
        {
            return new RefundTransactionResult
            {
                IsSuccessful = false,
                OriginalTransactionId = request.OriginalTransactionId,
                ErrorMessage = "تراکنش اصلی یافت نشد"
            };
        }

        // Validate transaction ownership
        if (originalTransaction.UserId != request.UserId)
        {
            return new RefundTransactionResult
            {
                IsSuccessful = false,
                OriginalTransactionId = request.OriginalTransactionId,
                ErrorMessage = "شما مجاز به استرداد این تراکنش نیستید"
            };
        }

        // Check if transaction is refundable
        if (!originalTransaction.IsRefundable())
        {
            return new RefundTransactionResult
            {
                IsSuccessful = false,
                OriginalTransactionId = request.OriginalTransactionId,
                ErrorMessage = "این تراکنش قابل استرداد نیست"
            };
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            // Get wallet and currency account
            var wallet = await _unitOfWork.Wallets.GetByUserIdWithIncludesAsync(
                request.UserId,
                includeCurrencyAccounts: true,
                cancellationToken: ct);

            if (wallet == null)
            {
                throw new WalletNotFoundException(request.UserId);
            }

            var account = wallet.GetCurrencyAccount(originalTransaction.Amount.Currency);
            if (account == null)
            {
                throw new InvalidCurrencyAccountException(originalTransaction.Amount.Currency);
            }

            // Calculate refund amount
            var refundAmount = request.PartialAmount ?? originalTransaction.Amount.Value;
            if (refundAmount > originalTransaction.Amount.Value)
            {
                throw new InvalidTransactionException("مبلغ استرداد نمی‌تواند بیش از مبلغ اصلی باشد");
            }

            // Create refund transaction
            var refundTransaction = Transaction.CreateRefundTransaction(
                wallet.Id,
                account.Id,
                request.UserId,
                originalTransaction.Amount.Currency == account.Currency
                    ? Money.Create(refundAmount, originalTransaction.Amount.Currency)
                    : throw new InvalidCurrencyException(originalTransaction.Amount.Currency.ToString()),
                $"استرداد تراکنش {originalTransaction.TransactionNumber.Value} - {request.Reason}",
                originalTransaction.Id);

            // Process the refund
            account.ProcessRefund(refundTransaction);

            // Save changes
            await _unitOfWork.Transactions.AddAsync(refundTransaction, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return new RefundTransactionResult
            {
                IsSuccessful = true,
                RefundTransactionId = refundTransaction.Id,
                OriginalTransactionId = originalTransaction.Id,
                RefundAmount = refundAmount,
                NewWalletBalance = account.Balance.Value,
                ProcessedAt = DateTime.UtcNow
            };
        }, cancellationToken);
    }
}