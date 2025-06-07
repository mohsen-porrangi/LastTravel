using BuildingBlocks.Messaging.Handlers;
using Microsoft.Extensions.Logging;
using WalletPayment.Domain.Events;

namespace WalletPayment.Application.EventHandlers.Internal;

/// <summary>
/// Handler for refund completed events
/// ✅ Internal processing for refunds
/// </summary>
public class RefundCompletedEventHandler : IIntegrationEventHandler<RefundCompletedEvent>
{
    private readonly ILogger<RefundCompletedEventHandler> _logger;

    public RefundCompletedEventHandler(ILogger<RefundCompletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(RefundCompletedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Refund completed: Original: {OriginalTransactionId} - Refund: {RefundTransactionId} - Amount: {Amount} {Currency}",
            @event.OriginalTransactionId, @event.RefundTransactionId, @event.Amount, @event.Currency);

        // TODO: Add refund-specific processing:
        // - Send refund notification to user
        // - Update order status if linked
        // - Log for financial reconciliation
        // - Update customer service records

        await Task.CompletedTask;
    }
}