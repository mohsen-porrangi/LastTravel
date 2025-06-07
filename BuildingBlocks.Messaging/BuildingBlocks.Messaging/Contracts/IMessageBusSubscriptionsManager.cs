using BuildingBlocks.Messaging.Events;
using BuildingBlocks.Messaging.Handlers;

namespace BuildingBlocks.Messaging.Contracts;
/// <summary>
/// رابط برای مدیریت اشتراک‌های باس پیام
/// </summary>
public interface IMessageBusSubscriptionsManager
{
    /// <summary>
    /// افزودن یک اشتراک برای یک نوع رویداد خاص
    /// </summary>
    void AddSubscription<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>;

    /// <summary>
    /// بررسی وجود اشتراک برای یک نوع رویداد خاص
    /// </summary>
    bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent;

    /// <summary>
    /// بررسی وجود اشتراک برای یک نوع رویداد با نام مشخص
    /// </summary>
    bool HasSubscriptionsForEvent(string eventName);

    /// <summary>
    /// دریافت تمام هندلرهای یک نوع رویداد خاص
    /// </summary>
    IEnumerable<Type> GetHandlersForEvent<T>() where T : IntegrationEvent;

    /// <summary>
    /// دریافت تمام هندلرهای یک نوع رویداد با نام مشخص
    /// </summary>
    IEnumerable<Type> GetHandlersForEvent(string eventName);

    /// <summary>
    /// دریافت نام یک نوع رویداد
    /// </summary>
    string GetEventKey<T>();

    // public void RemoveSubscription<T, TH>()  // حذف یک اشتراک برای زمانی که نیاز به مدیریت پویای اشتراک‌ها داریم
    //    where T : IntegrationEvent
    //    where TH : IIntegrationEventHandler<T>;

    // public void Clear();  // پاک‌سازی تمام اشتراک‌ها برای مواقعی که نیاز به راه‌اندازی مجدد داریم
}