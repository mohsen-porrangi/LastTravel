using BuildingBlocks.Messaging.Events;
using BuildingBlocks.Messaging.Handlers;

namespace Order.Application.EventHandlers;

// Event from Wallet service when wallet is charged
public record WalletChargedEvent : IntegrationEvent
{
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public Guid? OrderId { get; init; }
}

public class WalletChargedEventHandler : IIntegrationEventHandler<WalletChargedEvent>
{
    public Task HandleAsync(WalletChargedEvent @event, CancellationToken cancellationToken)
    {
        // If the charge was for a specific order, we might need to retry payment
        // This is a simplified implementation
        return Task.CompletedTask;
    }
}