using BuildingBlocks.Messaging.Handlers;
using Microsoft.Extensions.Logging;
using WalletPayment.Application.EventHandlers;
using WalletPayment.Domain.Events;

namespace WalletPayment.Application.EventHandlers.Internal;

/// <summary>
/// Handler for internal transaction completed events
/// ✅ For internal wallet operations and audit logging
/// </summary>
public class InternalTransactionCompletedEventHandler : IIntegrationEventHandler<TransactionCompletedEvent>
{
    private readonly ILogger<InternalTransactionCompletedEventHandler> _logger;

    public InternalTransactionCompletedEventHandler(ILogger<InternalTransactionCompletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(TransactionCompletedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Transaction completed: {TransactionId} - {TransactionNumber} - Amount: {Amount} {Currency} - User: {UserId}",
            @event.TransactionId, @event.TransactionNumber, @event.Amount, @event.Currency, @event.UserId);

        // TODO: Add additional internal processing if needed:
        // - Update analytics
        // - Send notifications
        // - Update user metrics
        // - Log for compliance/audit

        await Task.CompletedTask;
    }
}
