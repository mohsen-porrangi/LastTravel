using BuildingBlocks.Messaging.Contracts;
using BuildingBlocks.Messaging.InMemory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks.Messaging.Extensions;

/// <summary>
/// متدهای کمکی برای ثبت و پیکربندی سرویس‌های پیام‌رسانی
/// </summary>
public static class MessagingServiceCollectionExtensions
{
    /// <summary>
    /// افزودن زیرساخت پیام‌رسانی به سرویس‌ها
    /// </summary>
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly? handlersAssembly = null)
    {
        // در مونولیت، از پیاده‌سازی درون‌حافظه‌ای استفاده می‌کنیم
        services.AddSingleton<IMessageBusSubscriptionsManager, InMemoryMessageBusSubscriptionsManager>();
        services.AddSingleton<IMessageBus, InMemoryMessageBus>();

        // اگر assembly هندلرها مشخص شده باشد، آنها را به صورت خودکار ثبت می‌کنیم
        if (handlersAssembly != null)
        {
            services.RegisterEventHandlers(handlersAssembly);
        }

        return services;
    }
}