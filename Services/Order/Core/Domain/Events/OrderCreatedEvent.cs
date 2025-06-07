using BuildingBlocks.Messaging.Events;
using Order.Domain.Enums;

namespace Order.Domain.Events;

public record OrderCreatedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public ServiceType ServiceType { get; init; }
    public string? OrderNumber { get; init; }

    public OrderCreatedEvent(Guid orderId, Guid userId, ServiceType serviceType, string? OrderNumber)
    {
        OrderId = orderId;
        UserId = userId;
        ServiceType = serviceType;
        Source = "OrderService";
    }
}