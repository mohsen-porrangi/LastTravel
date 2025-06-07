using BuildingBlocks.Messaging.Events;

namespace Order.Application.IntegrationEvents.Events;

public record OrderPaymentRequestedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public string OrderNumber { get; init; }

    public OrderPaymentRequestedIntegrationEvent(Guid orderId, Guid userId, decimal amount, string orderNumber)
    {
        OrderId = orderId;
        UserId = userId;
        Amount = amount;
        OrderNumber = orderNumber;
        Source = "OrderService";
    }
}