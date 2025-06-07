namespace BuildingBlocks.Messaging.InMemory;

using BuildingBlocks.Messaging.Contracts;
using BuildingBlocks.Messaging.Events;
using BuildingBlocks.Messaging.Handlers;
/// <summary>
/// پیاده‌سازی درون‌حافظه‌ای برای مدیریت اشتراک‌های باس پیام
/// </summary>
public class InMemoryMessageBusSubscriptionsManager : IMessageBusSubscriptionsManager
{
    private readonly Dictionary<string, List<Type>> _handlers = new();

    public void AddSubscription<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
        var eventName = GetEventKey<T>();

        if (!HasSubscriptionsForEvent(eventName))
        {
            _handlers.Add(eventName, new List<Type>());
        }

        if (_handlers[eventName].Contains(typeof(TH)))
        {
            Console.WriteLine($"Handler {typeof(TH).Name} already registered for {eventName}");
            throw new ArgumentException($"هندلر {typeof(TH).Name} قبلاً برای رویداد {eventName} ثبت شده است", nameof(eventName));

        }

        _handlers[eventName].Add(typeof(TH));
    }

    public bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent
    {
        var eventName = GetEventKey<T>();
        return HasSubscriptionsForEvent(eventName);
    }

    public bool HasSubscriptionsForEvent(string eventName)
    {
        return _handlers.ContainsKey(eventName);
    }

    public IEnumerable<Type> GetHandlersForEvent<T>() where T : IntegrationEvent
    {
        var eventName = GetEventKey<T>();
        return GetHandlersForEvent(eventName);
    }

    public IEnumerable<Type> GetHandlersForEvent(string eventName)
    {
        return _handlers.ContainsKey(eventName) ? _handlers[eventName] : Enumerable.Empty<Type>();
    }

    public string GetEventKey<T>()
    {
        return typeof(T).Name;
    }
}