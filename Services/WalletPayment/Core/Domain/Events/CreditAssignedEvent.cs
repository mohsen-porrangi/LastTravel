using BuildingBlocks.Messaging.Events;

namespace WalletPayment.Domain.Events;

/// <summary>
/// Credit assigned event (B2B)
/// </summary>
public record CreditAssignedEvent : IntegrationEvent
{
    public CreditAssignedEvent(Guid walletId, Guid userId, decimal amount, DateTime dueDate)
    {
        WalletId = walletId;
        UserId = userId;
        Amount = amount;
        DueDate = dueDate;
        Source = "WalletPayment";
    }

    public Guid WalletId { get; }
    public Guid UserId { get; }
    public decimal Amount { get; }
    public DateTime DueDate { get; }
}

/// <summary>
/// Credit settled event (B2B)
/// </summary>
public record CreditSettledEvent : IntegrationEvent
{
    public CreditSettledEvent(Guid walletId, Guid transactionId, decimal settledAmount)
    {
        WalletId = walletId;
        TransactionId = transactionId;
        SettledAmount = settledAmount;
        SettledAt = DateTime.UtcNow;
        Source = "WalletPayment";
    }

    public Guid WalletId { get; }
    public Guid TransactionId { get; }
    public decimal SettledAmount { get; }
    public DateTime SettledAt { get; }
}