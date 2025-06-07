namespace BuildingBlocks.Messaging.Events;

/// <summary>
/// رکورد پایه برای تمام رویدادهای یکپارچه‌سازی
/// </summary>
public abstract record IntegrationEvent
{
    // یک شناسه منحصر به فرد برای هر نمونه رویداد
    public Guid Id { get; } = Guid.NewGuid();

    // زمان رخداد رویداد
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    // نوع رویداد برای سریالایز/دیسریالایز
    public string EventType => GetType().AssemblyQualifiedName!;

    // منبع رویداد (ماژول یا سرویسی که رویداد از آن نشأت گرفته است)
    public string Source { get; init; } = string.Empty;

    // public virtual string Version => "1.0";  // نسخه رویداد برای پشتیبانی از تکامل و سازگاری آینده
    // public string? CorrelationId { get; init; }  // شناسه همبستگی برای ردیابی جریان درخواست در چندین سرویس
    // public string? CausationId { get; init; }  // شناسه علیت برای مشخص کردن رویداد قبلی که باعث این رویداد شده است
    // public IDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();  // متاداده اضافی برای انعطاف‌پذیری بیشتر
}