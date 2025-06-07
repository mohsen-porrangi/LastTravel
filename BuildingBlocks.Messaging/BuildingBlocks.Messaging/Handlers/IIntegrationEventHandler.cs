using BuildingBlocks.Messaging.Events;

namespace BuildingBlocks.Messaging.Handlers;
/// <summary>
/// رابط برای هندلرهای رویدادهای یکپارچه‌سازی
/// </summary>
public interface IIntegrationEventHandler<in TEvent> where TEvent : IntegrationEvent
{
    /// <summary>
    /// پردازش یک رویداد یکپارچه‌سازی
    /// </summary>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}