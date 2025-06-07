using BuildingBlocks.Messaging.Events;

namespace BuildingBlocks.Messaging.Contracts;
/// <summary>
/// رابط باس پیام برای ارسال و دریافت رویدادها
/// </summary>
public interface IMessageBus
{
    /// <summary>
    /// انتشار یک رویداد به باس پیام
    /// </summary>
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : IntegrationEvent;

    /// <summary>
    /// ارسال یک دستور به یک گیرنده خاص
    /// </summary>
    Task SendAsync<T>(T message, CancellationToken cancellationToken = default) where T : IntegrationEvent;

    // public Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)  // برای الگوی درخواست-پاسخ در ارتباطات همزمان سرویس
    //     where TRequest : IntegrationEvent;
}