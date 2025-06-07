namespace BuildingBlocks.Messaging.InMemory;

using BuildingBlocks.Messaging.Contracts;
using BuildingBlocks.Messaging.Events;
using BuildingBlocks.Messaging.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// پیاده‌سازی درون‌حافظه‌ای باس پیام برای استفاده در مونولیت
/// </summary>
public class InMemoryMessageBus(
    IServiceProvider serviceProvider,
    IMessageBusSubscriptionsManager subscriptionsManager,
    ILogger<InMemoryMessageBus> logger
) : IMessageBus
{

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : IntegrationEvent
    {
        var actualEventType = message.GetType();
        var eventName = actualEventType.Name; // Use actual type

        logger.LogInformation("Publishing event {EventName} with ID {EventId}", eventName, message.Id);

        if (!subscriptionsManager.HasSubscriptionsForEvent(eventName))
        {
            logger.LogWarning("No subscriptions found for event {EventName}", eventName);
            return;
        }

        var handlers = subscriptionsManager.GetHandlersForEvent(eventName);

        using var scope = serviceProvider.CreateScope();

        foreach (var handlerType in handlers)
        {
            var handler = scope.ServiceProvider.GetService(handlerType);

            if (handler == null)
            {
                logger.LogWarning("Handler {HandlerType} not found for event {EventName}", handlerType.Name, eventName);
                continue;
            }

            logger.LogDebug("Processing event {EventName} with handler {HandlerType}", eventName, handlerType.Name);

            var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(actualEventType); // Use actual type
            var handleMethod = concreteType.GetMethod("HandleAsync");

            if (handleMethod != null)
            {
                await (Task)handleMethod.Invoke(handler, new object[] { message, cancellationToken });
            }
        }
    }

    public Task SendAsync<T>(T message, CancellationToken cancellationToken = default) where T : IntegrationEvent
    {
        // در پیاده‌سازی درون‌حافظه‌ای، تفاوتی بین انتشار و ارسال وجود ندارد
        return PublishAsync(message, cancellationToken);
    }
}