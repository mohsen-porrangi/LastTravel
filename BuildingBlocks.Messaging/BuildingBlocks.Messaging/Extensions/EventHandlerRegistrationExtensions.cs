using BuildingBlocks.Messaging.Handlers;
using BuildingBlocks.Messaging.Registration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks.Messaging.Extensions;

/// <summary>
/// متدهای کمکی برای ثبت خودکار هندلرهای رویداد
/// ✅ Fixed: Prevents duplicate registrations
/// </summary>
public static class EventHandlerRegistrationExtensions
{
    // ✅ Static HashSet to track registered combinations
    private static readonly HashSet<string> RegisteredHandlers = new();

    /// <summary>
    /// ثبت خودکار هندلرهای رویداد از assembly مشخص شده
    /// </summary>
    /// <param name="services">مجموعه سرویس‌ها</param>
    /// <param name="assembly">assembly حاوی هندلرهای رویداد</param>
    /// <returns>مجموعه سرویس‌های به‌روزشده</returns>
    public static IServiceCollection RegisterEventHandlers(this IServiceCollection services, Assembly assembly)
    {
        // یافتن تمام کلاس‌هایی که IIntegrationEventHandler را پیاده‌سازی می‌کنند
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)))
            .ToList();

        Console.WriteLine($"Found {handlerTypes.Count} handler types in {assembly.GetName().Name}");

        foreach (var handlerType in handlerTypes)
        {
            // یافتن رابط IIntegrationEventHandler که این کلاس پیاده‌سازی می‌کند
            var handlerInterface = handlerType.GetInterfaces()
                .First(i => i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>));

            // دریافت نوع رویداد مرتبط با این هندلر
            var eventType = handlerInterface.GetGenericArguments()[0];

            // ✅ Create unique key for this event-handler combination
            var registrationKey = $"{eventType.FullName}|{handlerType.FullName}";

            // ✅ Skip if already registered
            if (RegisteredHandlers.Contains(registrationKey))
            {
                Console.WriteLine($"Skipping duplicate: {eventType.Name} -> {handlerType.Name}");
                continue;
            }

            // ✅ Mark as registered
            RegisteredHandlers.Add(registrationKey);

            Console.WriteLine($"Registering: {eventType.Name} -> {handlerType.Name}");

            // ثبت هندلر در DI
            services.AddTransient(handlerType);

            // ثبت یک راه‌انداز (startup task) برای پیکربندی اشتراک‌ها
            services.AddSingleton(new EventHandlerRegistration(eventType, handlerType));
        }

        // ✅ Only add hosted service once per application
        if (!services.Any(s => s.ServiceType == typeof(EventHandlerRegistrationService)))
        {
            services.AddHostedService<EventHandlerRegistrationService>();
            Console.WriteLine("Added EventHandlerRegistrationService");
        }

        return services;
    }

    /// <summary>
    /// Clear registered handlers (for testing purposes)
    /// </summary>
    public static void ClearRegistrations()
    {
        RegisteredHandlers.Clear();
    }
}