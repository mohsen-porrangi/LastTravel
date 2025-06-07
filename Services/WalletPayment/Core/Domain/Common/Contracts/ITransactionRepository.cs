using BuildingBlocks.Contracts;
using WalletPayment.Domain.TransactionAggregate;
using WalletPayment.Domain.ValueObjects;
using WalletPayment.Domain.Enums;
using WalletPayment.Domain.Aggregates.TransactionAggregate;

namespace WalletPayment.Domain.Common.Contracts;

/// <summary>
/// Repository contract for Transaction aggregate
/// </summary>
public interface ITransactionRepository : IRepositoryBase<Transaction, Guid>
{
    /// <summary>
    /// Get transaction by transaction number
    /// </summary>
    Task<Transaction?> GetByTransactionNumberAsync(
        TransactionNumber transactionNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get transaction by payment reference ID
    /// </summary>
    Task<Transaction?> GetByPaymentReferenceAsync(
        string paymentReferenceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user transactions with pagination and filtering
    /// </summary>
    Task<(IEnumerable<Transaction> Transactions, int TotalCount)> GetUserTransactionsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        TransactionType? type = null,
        TransactionDirection? direction = null,
        CurrencyCode? currency = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get transactions for specific account
    /// </summary>
    Task<IEnumerable<Transaction>> GetAccountTransactionsAsync(
        Guid accountId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? limit = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get refundable transactions for user
    /// </summary>
    Task<IEnumerable<Transaction>> GetRefundableTransactionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get related transactions (transfers, refunds)
    /// </summary>
    Task<IEnumerable<Transaction>> GetRelatedTransactionsAsync(
        Guid originalTransactionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending transactions older than specified minutes
    /// </summary>
    Task<IEnumerable<Transaction>> GetPendingTransactionsOlderThanAsync(
        int minutes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if payment reference already exists
    /// </summary>
    Task<bool> PaymentReferenceExistsAsync(
        string paymentReferenceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get transaction statistics for period
    /// </summary>
    Task<TransactionStatistics> GetTransactionStatisticsAsync(
        Guid? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create transaction snapshot
    /// </summary>
    Task AddSnapshotAsync(TransactionSnapshot snapshot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get account balance snapshots
    /// </summary>
    Task<IEnumerable<TransactionSnapshot>> GetAccountSnapshotsAsync(
        Guid accountId,
        SnapshotType? type = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Transaction statistics DTO
/// </summary>
public record TransactionStatistics
{
    public int TotalTransactions { get; init; }
    public int SuccessfulTransactions { get; init; }
    public int FailedTransactions { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal TotalDeposits { get; init; }
    public decimal TotalWithdrawals { get; init; }
    public CurrencyCode Currency { get; init; }
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
}