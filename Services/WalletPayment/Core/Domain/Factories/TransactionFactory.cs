using WalletPayment.Domain.ValueObjects;
using WalletPayment.Domain.Enums;
using WalletPayment.Domain.Aggregates.TransactionAggregate;

namespace WalletPayment.Domain.Factories;

/// <summary>
/// Transaction factory for creating different types of transactions
/// </summary>
public static class TransactionFactory
{
    /// <summary>
    /// Create deposit transaction
    /// </summary>
    public static Transaction CreateDeposit(
        Guid walletId,
        Guid currencyAccountId,
        Guid userId,
        Money amount,
        string description,
        string? paymentReferenceId = null)
    {
        return Transaction.CreateDepositTransaction(
            walletId, currencyAccountId, userId, amount, description, paymentReferenceId);
    }

    /// <summary>
    /// Create purchase transaction
    /// </summary>
    public static Transaction CreatePurchase(
        Guid walletId,
        Guid currencyAccountId,
        Guid userId,
        Money amount,
        string description,
        string? orderContext = null)
    {
        return Transaction.CreatePurchaseTransaction(
            walletId, currencyAccountId, userId, amount, description, orderContext);
    }

    /// <summary>
    /// Create refund transaction
    /// </summary>
    public static Transaction CreateRefund(
        Guid walletId,
        Guid currencyAccountId,
        Guid userId,
        Money amount,
        string description,
        Guid originalTransactionId)
    {
        return Transaction.CreateRefundTransaction(
            walletId, currencyAccountId, userId, amount, description, originalTransactionId);
    }

    /// <summary>
    /// Create credit transaction (B2B)
    /// </summary>
    public static Transaction CreateCredit(
        Guid walletId,
        Guid currencyAccountId,
        Guid userId,
        Money amount,
        string description,
        DateTime dueDate,
        string? orderContext = null)
    {
        return Transaction.CreateCreditTransaction(
            walletId, currencyAccountId, userId, amount, description, dueDate, orderContext);
    }
}