using BuildingBlocks.Contracts;
using BuildingBlocks.Domain;
using WalletPayment.Domain.Enums;
using WalletPayment.Domain.Events;
using WalletPayment.Domain.Exceptions;
using WalletPayment.Domain.ValueObjects;
using WalletPayment.Domain.Aggregates.TransactionAggregate;

namespace WalletPayment.Domain.Aggregates.WalletAggregate;

/// <summary>
/// Currency Account Entity - Handles balance management for specific currency
/// </summary>
public class CurrencyAccount : EntityWithDomainEvents<Guid>, ISoftDelete
{
    private Money _balance = null!;
    private readonly List<Transaction> _transactions = new();

    public Guid WalletId { get; private set; }
    public CurrencyCode Currency { get; private set; }
    public Money Balance
    {
        get => _balance;
        private set => _balance = value ?? throw new ArgumentNullException(nameof(value));
    }
    public bool IsActive { get; private set; }

    // Navigation properties
    public virtual Wallet Wallet { get; private set; } = null!;
    public virtual IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    // Private constructor for EF Core
    private CurrencyAccount() { }

    /// <summary>
    /// Create new currency account
    /// </summary>
    public CurrencyAccount(Guid walletId, CurrencyCode currency)
    {
        if (walletId == Guid.Empty)
            throw new ArgumentException("WalletId cannot be empty", nameof(walletId));

        Id = Guid.NewGuid();
        WalletId = walletId;
        Currency = currency;
        Balance = Money.Zero(currency);
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Create deposit transaction
    /// </summary>
    public Transaction CreateDepositTransaction(Money amount, string description, string? paymentReferenceId = null)
    {
        EnsureActive();
        ValidateAmount(amount);

        var transaction = Transaction.CreateDepositTransaction(
            WalletId,
            Id,
            Wallet?.UserId ?? Guid.Empty,
            amount,
            description,
            paymentReferenceId);

        return transaction;
    }

    /// <summary>
    /// Create purchase transaction
    /// </summary>
    public Transaction CreatePurchaseTransaction(Money amount, string description, string? orderContext = null)
    {
        EnsureActive();
        ValidateAmount(amount);

        if (!HasSufficientBalance(amount.Value))
            throw new InsufficientBalanceException(WalletId, amount.Value, Balance.Value);

        var transaction = Transaction.CreatePurchaseTransaction(
            WalletId,
            Id,
            Wallet?.UserId ?? Guid.Empty,
            amount,
            description,
            orderContext);

        return transaction;
    }

    /// <summary>
    /// Process completed deposit (increase balance)
    /// </summary>
    public void ProcessDeposit(Transaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        if (transaction.CurrencyAccountId != Id)
            throw new InvalidOperationException("Transaction does not belong to this account");

        if (transaction.Direction != TransactionDirection.In)
            throw new InvalidOperationException("Only inbound transactions can be processed as deposits");

        if (transaction.Status != TransactionStatus.Pending)
            throw new InvalidOperationException("Only pending transactions can be processed");

        EnsureActive();

        // Update balance
        Balance = Balance.Add(transaction.Amount);
        UpdatedAt = DateTime.UtcNow;

        // Mark transaction as completed
        transaction.MarkAsCompleted();

        // Emit domain event
        AddDomainEvent(new WalletDepositedEvent(
            WalletId,
            Id,
            transaction.Amount.Value,
            Currency,
            transaction.PaymentReferenceId ?? string.Empty));
    }

    /// <summary>
    /// Process completed purchase (decrease balance)
    /// </summary>
    public void ProcessPurchase(Transaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        if (transaction.CurrencyAccountId != Id)
            throw new InvalidOperationException("Transaction does not belong to this account");

        if (transaction.Direction != TransactionDirection.Out)
            throw new InvalidOperationException("Only outbound transactions can be processed as purchases");

        EnsureActive();

        if (!HasSufficientBalance(transaction.Amount.Value))
            throw new InsufficientBalanceException(WalletId, transaction.Amount.Value, Balance.Value);

        // Update balance
        Balance = Balance.Subtract(transaction.Amount);
        UpdatedAt = DateTime.UtcNow;

        // Mark transaction as completed
        transaction.MarkAsCompleted();

        // Emit domain event
        AddDomainEvent(new WalletWithdrawnEvent(
            WalletId,
            Id,
            transaction.Amount.Value,
            Currency,
            transaction.OrderContext));
    }

    /// <summary>
    /// Process refund transaction
    /// </summary>
    public void ProcessRefund(Transaction refundTransaction)
    {
        if (refundTransaction == null)
            throw new ArgumentNullException(nameof(refundTransaction));

        if (refundTransaction.Type != TransactionType.Refund)
            throw new InvalidOperationException("Transaction must be a refund type");

        EnsureActive();

        // Update balance
        Balance = Balance.Add(refundTransaction.Amount);
        UpdatedAt = DateTime.UtcNow;

        // Mark transaction as completed
        refundTransaction.MarkAsCompleted();

        // Emit domain event
        AddDomainEvent(new RefundCompletedEvent(
            refundTransaction.RelatedTransactionId ?? Guid.Empty,
            refundTransaction.Id,
            WalletId,
            refundTransaction.Amount,
            Balance.Value));
    }

    /// <summary>
    /// Check if account has sufficient balance
    /// </summary>
    public bool HasSufficientBalance(decimal amount)
    {
        return IsActive && !IsDeleted && Balance.Value >= amount;
    }

    /// <summary>
    /// Activate account
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivate account
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Soft delete account
    /// </summary>
    public void SoftDelete()
    {
        if (Balance.Value > 0)
            throw new InvalidOperationException("Cannot delete account with positive balance");

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Get account summary
    /// </summary>
    public string GetSummary()
    {
        var status = IsActive ? "Active" : "Inactive";
        return $"{Currency}: {Balance.Value:N0} - {status}";
    }
    /// <summary>
    /// Process transfer transaction (both in/out)
    /// </summary>
    public void ProcessTransfer(Transaction transaction, Money actualAmount)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        if (transaction.CurrencyAccountId != Id)
            throw new InvalidOperationException("Transaction does not belong to this account");

        if (transaction.Type != TransactionType.Transfer)
            throw new InvalidOperationException("Only transfer transactions can be processed as transfers");

        EnsureActive();

        if (transaction.Direction == TransactionDirection.Out)
        {
            // Outgoing transfer - deduct from balance
            if (!HasSufficientBalance(actualAmount.Value))
                throw new InsufficientBalanceException(WalletId, actualAmount.Value, Balance.Value);

            Balance = Balance.Subtract(actualAmount);
        }
        else
        {
            // Incoming transfer - add to balance
            Balance = Balance.Add(actualAmount);
        }

        UpdatedAt = DateTime.UtcNow;

        // Mark transaction as completed
        transaction.MarkAsCompleted();

        // Emit domain events
        if (transaction.Direction == TransactionDirection.Out)
        {
            AddDomainEvent(new TransferInitiatedEvent(
                transaction.Id,
                WalletId,
                actualAmount,
                transaction.OrderContext ?? "Unknown"));
        }
        else
        {
            AddDomainEvent(new TransferCompletedEvent(
                transaction.RelatedTransactionId ?? Guid.Empty,
                transaction.Id,
                WalletId,
                actualAmount,
                Balance.Value));
        }
    }
    #region Private Methods

    private void ValidateAmount(Money amount)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));

        if (amount.Currency != Currency)
            throw new ArgumentException($"Amount currency {amount.Currency} does not match account currency {Currency}");

        if (amount.Value <= 0)
            throw new ArgumentException("Amount must be positive");
    }

    private void EnsureActive()
    {
        if (!IsActive)
            throw new InvalidOperationException("Account is not active");

        if (IsDeleted)
            throw new InvalidOperationException("Account has been deleted");
    }

    #endregion
}