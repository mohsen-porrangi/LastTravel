using BuildingBlocks.Domain;
using Order.Domain.Enums;

namespace Order.Domain.Entities;

public class OrderStatusHistory : BaseEntity<long>
{
    public Guid OrderId { get; private set; }
    public OrderStatus FromStatus { get; private set; }
    public OrderStatus ToStatus { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public Guid? ChangedBy { get; private set; }

    // Navigation
    public virtual Order Order { get; private set; } = null!;

    protected OrderStatusHistory() { }

    public OrderStatusHistory(Guid orderId, OrderStatus fromStatus, OrderStatus toStatus, string reason = "", Guid? changedBy = null)
    {
        OrderId = orderId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        Reason = reason;
        ChangedBy = changedBy;
        CreatedAt = DateTime.UtcNow;
    }
}