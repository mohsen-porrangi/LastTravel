namespace BuildingBlocks.Messaging.Registration;

/// <summary>
/// نگه‌دارنده اطلاعات ثبت هندلر رویداد
/// </summary>
public class EventHandlerRegistration(Type eventType, Type handlerType)
{
    /// <summary>
    /// نوع رویداد
    /// </summary>
    public Type EventType { get; } = eventType;

    /// <summary>
    /// نوع هندلر
    /// </summary>
    public Type HandlerType { get; } = handlerType;
}