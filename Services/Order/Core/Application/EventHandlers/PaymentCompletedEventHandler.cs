using BuildingBlocks.Messaging.Contracts;
using BuildingBlocks.Messaging.Events;
using BuildingBlocks.Messaging.Handlers;
using Order.Application.Orders.Services;

namespace Order.Application.EventHandlers;

// Event from Wallet service
public record PaymentCompletedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public long TransactionId { get; init; }
    public decimal Amount { get; init; }
}
public record OrderPaymentCompletedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }

    public OrderPaymentCompletedEvent(Guid orderId, Guid userId)
    {
        OrderId = orderId;
        UserId = userId;
        Source = "OrderService";
    }
}
public class PaymentCompletedEventHandler(
    OrderProcessingService orderProcessingService,
    IMessageBus messageBus
    )
    : IIntegrationEventHandler<PaymentCompletedEvent>
{
    public async Task HandleAsync(PaymentCompletedEvent @event, CancellationToken cancellationToken)
    {
        // Start ticket issuing process

        var order = await orderProcessingService.IssueTicketsAsync(@event, cancellationToken);
        if (order != null)
            await messageBus.PublishAsync(new OrderPaymentCompletedEvent(order.Id, order.UserId), cancellationToken);
    }
}