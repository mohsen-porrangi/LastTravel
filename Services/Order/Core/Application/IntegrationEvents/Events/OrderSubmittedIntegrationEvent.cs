using BuildingBlocks.Messaging.Events;
using Order.Domain.Enums;

namespace Order.Application.IntegrationEvents.Events;

public record OrderSubmittedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public ServiceType ServiceType { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public OrderSubmittedIntegrationEvent(Guid orderId, Guid userId, decimal amount, ServiceType serviceType, string orderNumber)
    {
        OrderId = orderId;
        UserId = userId;
        Amount = amount;
        ServiceType = serviceType;
        OrderNumber = orderNumber;
        Source = "OrderService";
    }
}