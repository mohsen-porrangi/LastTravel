using BuildingBlocks.Messaging.Events;

namespace Order.Domain.Events;

public record OrderCancelledEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public string Reason { get; init; }

    public OrderCancelledEvent(Guid orderId, Guid userId, string reason)
    {
        OrderId = orderId;
        UserId = userId;
        Reason = reason;
        Source = "OrderService";
    }
}