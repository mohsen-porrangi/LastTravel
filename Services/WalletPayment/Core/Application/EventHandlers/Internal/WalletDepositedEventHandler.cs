using BuildingBlocks.Messaging.Handlers;
using Microsoft.Extensions.Logging;
using WalletPayment.Domain.Events;

namespace WalletPayment.Application.EventHandlers.Internal;

/// <summary>
/// Handler for wallet deposited events
/// ✅ Internal processing for deposits
/// </summary>
public class WalletDepositedEventHandler : IIntegrationEventHandler<WalletDepositedEvent>
{
    private readonly ILogger<WalletDepositedEventHandler> _logger;

    public WalletDepositedEventHandler(ILogger<WalletDepositedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(WalletDepositedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Wallet deposit processed: WalletId: {WalletId} - Amount: {Amount} {Currency} - Reference: {ReferenceId}",
            @event.WalletId, @event.Amount, @event.Currency, @event.ReferenceId);

        // TODO: Add deposit-specific processing:
        // - Update user loyalty points
        // - Check for bonus eligibility
        // - Send deposit confirmation
        // - Update financial reports

        await Task.CompletedTask;
    }
}