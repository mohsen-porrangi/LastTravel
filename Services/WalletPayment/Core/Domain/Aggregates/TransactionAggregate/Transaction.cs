using BuildingBlocks.Domain;
using WalletPayment.Domain.Aggregates.WalletAggregate;
using WalletPayment.Domain.Enums;
using WalletPayment.Domain.Events;
using WalletPayment.Domain.ValueObjects;

namespace WalletPayment.Domain.Aggregates.TransactionAggregate;

/// <summary>
/// Transaction Entity - Core business entity for all financial operations
/// </summary>
public class Transaction : EntityWithDomainEvents<Guid>, IAggregateRoot
{
    // Private backing fields
    private TransactionNumber _transactionNumber = null!;
    private Money _amount = null!;

    // Public properties with business validation
    public TransactionNumber TransactionNumber
    {
        get => _transactionNumber;
        private set => _transactionNumber = value ?? throw new ArgumentNullException(nameof(value));
    }

    public Guid WalletId { get; private set; }
    public Guid CurrencyAccountId { get; private set; }
    public Guid? RelatedTransactionId { get; private set; }
    public Guid UserId { get; private set; } // Added for better querying

    public Money Amount
    {
        get => _amount;
        private set => _amount = value ?? throw new ArgumentNullException(nameof(value));
    }

    public TransactionDirection Direction { get; private set; }
    public TransactionType Type { get; private set; }
    public TransactionStatus Status { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public bool IsCredit { get; private set; }
    public DateTime? DueDate { get; private set; }
    public string? PaymentReferenceId { get; private set; }
    public string? OrderContext { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    // Navigation properties
    public virtual Wallet Wallet { get; private set; } = null!;
    public virtual CurrencyAccount CurrencyAccount { get; private set; } = null!;

    // Private constructor for EF Core
    private Transaction() { }

    #region Transfer
    /// <summary>
    /// Create transfer out transaction (sender side)
    /// </summary>
    public static Transaction CreateTransferOutTransaction(
        Guid walletId,
        Guid currencyAccountId,
        Guid userId,
        Money amount,
        string description,
        string transferReference)
    {
        var transaction = new Transaction(
            walletId: walletId,
            currencyAccountId: currencyAccountId,
            userId: userId,
            amount: amount,
            direction: TransactionDirection.Out,
            type: TransactionType.Transfer,
            description: description,
            orderContext: transferReference);

        return transaction;
    }
    /// <summary>
    /// Create transfer in transaction (receiver side)
    /// </summary>
    public static Transaction CreateTransferInTransaction(
        Guid walletId,
        Guid currencyAccountId,
        Guid userId,
        Money amount,
        string description,
        string transferReference)
    {
        var transaction = new Transaction(
            walletId: walletId,
            currencyAccountId: currencyAccountId,
            userId: userId,
            amount: amount,
            direction: TransactionDirection.In,
            type: TransactionType.Transfer,
            description: description,
            orderContext: transferReference);

        return transaction;
    }
    #endregion
    /// <summary>
    /// Create fee transaction
    /// </summary>
    public static Transaction CreateFeeTransaction(
        Guid walletId,
        Guid currencyAccountId,
        Guid userId,
        Money feeAmount,
        string description,
        Guid relatedTransactionId)
    {
        var transaction = new Transaction(
            walletId: walletId,
            currencyAccountId: currencyAccountId,
            userId: userId,
            amount: feeAmount,
            direction: TransactionDirection.Out,
            type: TransactionType.Fee,
            description: description,
            relatedTransactionId: relatedTransactionId);

        return transaction;
    }

    /// <summary>
    /// Set related transaction (for transfers)
    /// </summary>
    public void SetRelatedTransaction(Guid relatedTransactionId)
    {
        RelatedTransactionId = relatedTransactionId;
        UpdatedAt = DateTime.UtcNow;
    }



    /// <summary>
    /// Factory method - Create deposit transaction
    /// </summary>
    public static Transaction CreateDepositTransaction(
        Guid walletId,
        Guid currencyAccountId,
        Guid userId,
        Money amount,
        string description,
        string? paymentReferenceId = null)
    {
        return new Transaction(
            walletId: walletId,
            currencyAccountId: currencyAccountId,
            userId: userId,
            amount: amount,
            direction: TransactionDirection.In,
            type: TransactionType.Deposit,
            description: description,
            paymentReferenceId: paymentReferenceId);
    }

    /// <summary>
    /// Factory method - Create purchase transaction
    /// </summary>
    public static Transaction CreatePurchaseTransaction(
        Guid walletId,
        Guid currencyAccountId,
        Guid userId,
        Money amount,
        string description,
        string? orderContext = null)
    {
        return new Transaction(
            walletId: walletId,
            currencyAccountId: currencyAccountId,
            userId: userId,
            amount: amount,
            direction: TransactionDirection.Out,
            type: TransactionType.Purchase,
            description: description,
            orderContext: orderContext);
    }

    /// <summary>
    /// Factory method - Create refund transaction
    /// </summary>
    public static Transaction CreateRefundTransaction(
        Guid walletId,
        Guid currencyAccountId,
        Guid userId,
        Money amount,
        string description,
        Guid originalTransactionId)
    {
        return new Transaction(
            walletId: walletId,
            currencyAccountId: currencyAccountId,
            userId: userId,
            amount: amount,
            direction: TransactionDirection.In,
            type: TransactionType.Refund,
            description: description,
            relatedTransactionId: originalTransactionId);
    }

    /// <summary>
    /// Factory method - Create credit transaction (B2B)
    /// </summary>
    public static Transaction CreateCreditTransaction(
        Guid walletId,
        Guid currencyAccountId,
        Guid userId,
        Money amount,
        string description,
        DateTime dueDate,
        string? orderContext = null)
    {
        return new Transaction(
            walletId: walletId,
            currencyAccountId: currencyAccountId,
            userId: userId,
            amount: amount,
            direction: TransactionDirection.Out,
            type: TransactionType.Purchase,
            description: description,
            isCredit: true,
            dueDate: dueDate,
            orderContext: orderContext);
    }

    /// <summary>
    /// Private constructor with validation
    /// </summary>
    private Transaction(
        Guid walletId,
        Guid currencyAccountId,
        Guid userId,
        Money amount,
        TransactionDirection direction,
        TransactionType type,
        string description,
        bool isCredit = false,
        DateTime? dueDate = null,
        string? paymentReferenceId = null,
        Guid? relatedTransactionId = null,
        string? orderContext = null)
    {
        // Validation
        ValidateInputs(walletId, currencyAccountId, userId, amount, description, type, direction);

        // Set properties
        Id = Guid.NewGuid();
        WalletId = walletId;
        CurrencyAccountId = currencyAccountId;
        UserId = userId;
        Amount = amount;
        Direction = direction;
        Type = type;
        Status = TransactionStatus.Pending;
        Description = description.Trim();
        IsCredit = isCredit;
        DueDate = dueDate;
        PaymentReferenceId = paymentReferenceId;
        RelatedTransactionId = relatedTransactionId;
        OrderContext = orderContext;
        TransactionDate = DateTime.UtcNow;

        // Generate transaction number
        TransactionNumber = TransactionNumber.Generate();

        // Initialize timestamps
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsDeleted = false;

        // Emit domain event
        AddDomainEvent(new TransactionInitiatedEvent(
            Id, WalletId, Amount, Direction, Type, OrderContext));
    }

    /// <summary>
    /// Mark transaction as completed
    /// </summary>
    public void MarkAsCompleted()
    {
        if (Status == TransactionStatus.Completed)
            return;

        Status = TransactionStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;        
        AddDomainEvent(new TransactionCompletedEvent(
            Id,
            TransactionNumber.Value,
            WalletId,
            UserId, 
            Amount.Value,
            Direction,
            Type,
            Amount.Currency,
            Description,
            PaymentReferenceId,
            OrderContext));
    }

    /// <summary>
    /// Mark transaction as failed
    /// </summary>
    public void MarkAsFailed(string? reason = null)
    {
        if (Status == TransactionStatus.Completed)
            throw new InvalidOperationException("Cannot fail a completed transaction");

        Status = TransactionStatus.Failed;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(reason))
        {
            Description = $"{Description} - Failed: {reason}";
        }
        
        AddDomainEvent(new TransactionFailedEvent(Id, WalletId, reason ?? "Unknown error"));
    }

    /// <summary>
    /// Set payment reference for gateway transactions
    /// </summary>
    public void SetPaymentReference(string paymentReferenceId)
    {
        if (string.IsNullOrWhiteSpace(paymentReferenceId))
            throw new ArgumentException("Payment reference cannot be empty", nameof(paymentReferenceId));

        PaymentReferenceId = paymentReferenceId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if transaction is refundable
    /// </summary>
    public bool IsRefundable()
    {
        return Status == TransactionStatus.Completed &&
               Direction == TransactionDirection.Out &&
               Type != TransactionType.Refund &&
               ProcessedAt.HasValue &&
               ProcessedAt.Value > DateTime.UtcNow.AddDays(-30); // 30 days policy
    }

    /// <summary>
    /// Get display amount (positive for display purposes)
    /// </summary>
    public decimal GetDisplayAmount()
    {
        return Math.Abs(Amount.Value);
    }

    #region Private Validation Methods

    private static void ValidateInputs(
        Guid walletId,
        Guid currencyAccountId,
        Guid userId,
        Money amount,
        string description,
        TransactionType type,
        TransactionDirection direction)
    {
        if (walletId == Guid.Empty)
            throw new ArgumentException("WalletId cannot be empty", nameof(walletId));

        if (currencyAccountId == Guid.Empty)
            throw new ArgumentException("CurrencyAccountId cannot be empty", nameof(currencyAccountId));

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));

        if (amount == null)
            throw new ArgumentNullException(nameof(amount));

        if (amount.Value <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));

        if (description.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters", nameof(description));

        ValidateTransactionTypeForDirection(type, direction);
    }

    private static void ValidateTransactionTypeForDirection(TransactionType type, TransactionDirection direction)
    {
        var validInboundTypes = new[] { TransactionType.Deposit, TransactionType.Refund };
        var validOutboundTypes = new[] { TransactionType.Purchase, TransactionType.Withdrawal, TransactionType.Transfer, TransactionType.Fee };

        if (direction == TransactionDirection.In && !validInboundTypes.Contains(type))
            throw new ArgumentException($"Transaction type {type} is not valid for inbound transactions");

        if (direction == TransactionDirection.Out && !validOutboundTypes.Contains(type))
            throw new ArgumentException($"Transaction type {type} is not valid for outbound transactions");
    }

    #endregion
}
