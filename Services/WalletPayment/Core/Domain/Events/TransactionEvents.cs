using BuildingBlocks.Messaging.Events;
using WalletPayment.Domain.ValueObjects;
using WalletPayment.Domain.Enums;

namespace WalletPayment.Domain.Events;

/// <summary>
/// Transaction initiated event
/// </summary>
public record TransactionInitiatedEvent : IntegrationEvent
{
    public TransactionInitiatedEvent(
        Guid transactionId,
        Guid walletId,
        Money amount,
        TransactionDirection direction,
        TransactionType type,
        string? orderContext = null)
    {
        TransactionId = transactionId;
        WalletId = walletId;
        Amount = amount.Value;
        Currency = amount.Currency;
        Direction = direction;
        Type = type;
        OrderContext = orderContext;
        Source = "WalletPayment";
    }

    public Guid TransactionId { get; }
    public Guid WalletId { get; }
    public decimal Amount { get; }
    public CurrencyCode Currency { get; }
    public TransactionDirection Direction { get; }
    public TransactionType Type { get; }
    public string? OrderContext { get; }
}

/// <summary>
/// Transaction completed event - Critical for Order Service communication
/// </summary>
public record TransactionCompletedEvent : IntegrationEvent
{
    public TransactionCompletedEvent(
        Guid transactionId,
        string transactionNumber,
        Guid walletId,
        Guid userId,
        decimal amount,
        TransactionDirection direction,
        TransactionType type,
        CurrencyCode currency,
        string description,
        string? paymentReferenceId = null,
        string? orderContext = null)
    {
        TransactionId = transactionId;
        TransactionNumber = transactionNumber;
        WalletId = walletId;
        UserId = userId;
        Amount = amount;
        Currency = currency;
        Direction = direction;
        Type = type;
        Description = description;
        PaymentReferenceId = paymentReferenceId;
        OrderContext = orderContext;
        CompletedAt = DateTime.UtcNow;
        Source = "WalletPayment";
    }

    public Guid TransactionId { get; }
    public string TransactionNumber { get; }
    public Guid WalletId { get; }
    public Guid UserId { get; }
    public decimal Amount { get; }
    public CurrencyCode Currency { get; }
    public TransactionDirection Direction { get; }
    public TransactionType Type { get; }
    public string Description { get; }
    public string? PaymentReferenceId { get; }
    public string? OrderContext { get; }
    public DateTime CompletedAt { get; }
}
/// <summary>
/// Transaction failed event
/// </summary>
public record TransactionFailedEvent : IntegrationEvent
{
    public TransactionFailedEvent(
        Guid transactionId,
        Guid walletId,
        string reason)
    {
        TransactionId = transactionId;
        WalletId = walletId;
        Reason = reason;
        FailedAt = DateTime.UtcNow;
        Source = "WalletPayment";
    }

    public Guid TransactionId { get; }
    public Guid WalletId { get; }
    public string Reason { get; }
    public DateTime FailedAt { get; }
}

/// <summary>
/// Refund initiated event
/// </summary>
public record RefundInitiatedEvent : IntegrationEvent
{
    public RefundInitiatedEvent(
        Guid originalTransactionId,
        Guid refundTransactionId,
        Guid walletId,
        Money amount,
        string reason)
    {
        OriginalTransactionId = originalTransactionId;
        RefundTransactionId = refundTransactionId;
        WalletId = walletId;
        Amount = amount.Value;
        Currency = amount.Currency;
        Reason = reason;
        Source = "WalletPayment";
    }

    public Guid OriginalTransactionId { get; }
    public Guid RefundTransactionId { get; }
    public Guid WalletId { get; }
    public decimal Amount { get; }
    public CurrencyCode Currency { get; }
    public string Reason { get; }
}

/// <summary>
/// Refund completed event
/// </summary>
public record RefundCompletedEvent : IntegrationEvent
{
    public RefundCompletedEvent(
        Guid originalTransactionId,
        Guid refundTransactionId,
        Guid walletId,
        Money amount,
        decimal newBalance)
    {
        OriginalTransactionId = originalTransactionId;
        RefundTransactionId = refundTransactionId;
        WalletId = walletId;
        Amount = amount.Value;
        Currency = amount.Currency;
        NewBalance = newBalance;
        RefundedAt = DateTime.UtcNow;
        Source = "WalletPayment";
    }

    public Guid OriginalTransactionId { get; }
    public Guid RefundTransactionId { get; }
    public Guid WalletId { get; }
    public decimal Amount { get; }
    public CurrencyCode Currency { get; }
    public decimal NewBalance { get; }
    public DateTime RefundedAt { get; }
}