using BuildingBlocks.Domain;
using Order.Domain.Enums;

namespace Order.Domain.Entities;

public class OrderWalletTransaction : BaseEntity<long>
{
    public Guid OrderId { get; private set; }
    public long TransactionId { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }

    // Navigation
    public virtual Order Order { get; private set; } = null!;

    protected OrderWalletTransaction() { }

    public OrderWalletTransaction(Guid orderId, long transactionId, TransactionType type, decimal amount)
    {
        OrderId = orderId;
        TransactionId = transactionId;
        Type = type;
        Amount = amount;
        CreatedAt = DateTime.UtcNow;
    }
}