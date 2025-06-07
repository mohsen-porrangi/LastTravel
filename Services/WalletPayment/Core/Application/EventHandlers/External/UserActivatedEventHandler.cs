using BuildingBlocks.Messaging.Events.UserEvents;
using BuildingBlocks.Messaging.Handlers;
using MediatR;
using Microsoft.Extensions.Logging;
using WalletPayment.Application.Features.Wallets.CreateWallet;

namespace WalletPayment.Application.EventHandlers.External;

/// <summary>
/// Handler for user activation from UserManagement service
/// ✅ Fixed: Proper event handler organization
/// </summary>
public class UserActivatedEventHandler : IIntegrationEventHandler<UserActivatedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserActivatedEventHandler> _logger;

    public UserActivatedEventHandler(
        IMediator mediator,
        ILogger<UserActivatedEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task HandleAsync(UserActivatedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing user activation for user {UserId}", @event.UserId);

        try
        {
            var command = new CreateWalletCommand
            {
                UserId = @event.UserId,
                CreateDefaultAccount = true
            };

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation(
                "Wallet created successfully for user {UserId}. WalletId: {WalletId}",
                @event.UserId, result.WalletId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create wallet for activated user {UserId}",
                @event.UserId);

            // Don't throw - wallet creation failure shouldn't break user activation
            // Could implement retry logic or manual intervention here
        }
    }
}