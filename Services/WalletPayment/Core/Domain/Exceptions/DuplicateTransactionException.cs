namespace WalletPayment.Domain.Exceptions;

using BuildingBlocks.Exceptions;

/// <summary>
/// خطای تراکنش تکراری
/// </summary>
public class DuplicateTransactionException : ConflictDomainException
{
    public DuplicateTransactionException(string transactionNumber)
        : base("تراکنش تکراری", $"تراکنش با شماره {transactionNumber} قبلاً ثبت شده است")
    {
        TransactionNumber = transactionNumber;
    }

    public string TransactionNumber { get; }
}