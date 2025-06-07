using BuildingBlocks.Domain;
using WalletPayment.Domain.ValueObjects;
using WalletPayment.Domain.Enums;

namespace WalletPayment.Domain.TransactionAggregate;

/// <summary>
/// Transaction Snapshot for balance history and reporting
/// </summary>
public class TransactionSnapshot : BaseEntity<long>
{
    public Guid AccountId { get; private set; }
    public Money Balance { get; private set; }
    public DateTime SnapshotDate { get; private set; }
    public SnapshotType Type { get; private set; }

    // For linking to specific transaction (optional)
    public Guid? TransactionId { get; private set; }

    // Private constructor for EF Core
    private TransactionSnapshot() { }

    /// <summary>
    /// Create account balance snapshot
    /// </summary>
    public TransactionSnapshot(
        Guid accountId,
        Money balance,
        SnapshotType type,
        Guid? transactionId = null)
    {
        if (accountId == Guid.Empty)
            throw new ArgumentException("AccountId cannot be empty", nameof(accountId));

        AccountId = accountId;
        Balance = balance ?? throw new ArgumentNullException(nameof(balance));
        Type = type;
        SnapshotDate = DateTime.UtcNow;
        TransactionId = transactionId;
    }

    /// <summary>
    /// Create daily snapshot
    /// </summary>
    public static TransactionSnapshot CreateDailySnapshot(Guid accountId, Money balance)
    {
        return new TransactionSnapshot(accountId, balance, SnapshotType.Daily);
    }

    /// <summary>
    /// Create manual snapshot
    /// </summary>
    public static TransactionSnapshot CreateManualSnapshot(Guid accountId, Money balance, Guid transactionId)
    {
        return new TransactionSnapshot(accountId, balance, SnapshotType.Manual, transactionId);
    }
}

