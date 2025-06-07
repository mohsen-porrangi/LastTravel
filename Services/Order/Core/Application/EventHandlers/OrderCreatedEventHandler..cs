using BuildingBlocks.Messaging.Contracts;
using BuildingBlocks.Messaging.Handlers;
using Order.Application.IntegrationEvents.Events;
using Order.Domain.Events;

namespace Order.Application.EventHandlers;

public class OrderCreatedEventHandler(IMessageBus messageBus)
    : IIntegrationEventHandler<OrderCreatedEvent>
{
    public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        // Publish integration event for other services
        var integrationEvent = new OrderSubmittedIntegrationEvent(
            @event.OrderId,
            @event.UserId,
            0, // Amount will be set later
            @event.ServiceType,
            @event.OrderNumber);

        await messageBus.PublishAsync(integrationEvent, cancellationToken);
    }
}