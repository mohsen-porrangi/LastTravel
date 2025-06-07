using BuildingBlocks.Messaging.Events;
using BuildingBlocks.Messaging.Handlers;
using Order.Domain.Contracts;
using Order.Domain.Enums;

namespace Order.Application.IntegrationEvents.Handlers;

// This would be an event from Wallet service
public record PaymentProcessedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public bool IsSuccess { get; init; }
    public string TransactionId { get; init; } = string.Empty;
}

public class PaymentProcessedIntegrationEventHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : IIntegrationEventHandler<PaymentProcessedIntegrationEvent>
{
    public async Task HandleAsync(PaymentProcessedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(@event.OrderId, track: true, cancellationToken);
        if (order == null) return;

        if (@event.IsSuccess)
        {
            order.UpdateStatus(OrderStatus.Processing, "Payment completed successfully");

            if (long.TryParse(@event.TransactionId, out var transactionId))
            {
                order.AddWalletTransaction(transactionId, TransactionType.Purchase, order.FullAmount);
            }
        }
        else
        {
            order.UpdateStatus(OrderStatus.Cancelled, "Payment failed");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}