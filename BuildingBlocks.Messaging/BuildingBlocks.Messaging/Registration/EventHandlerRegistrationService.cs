using BuildingBlocks.Messaging.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Messaging.Registration;

/// <summary>
/// سرویس راه‌انداز برای ثبت هندلرهای رویداد در زمان اجرا
/// ✅ Fixed: Prevents duplicate subscriptions
/// </summary>
public class EventHandlerRegistrationService(
    IServiceProvider serviceProvider,
    IEnumerable<EventHandlerRegistration> registrations) : IHostedService
{
    // ✅ Track processed registrations to prevent duplicates
    private static readonly HashSet<string> ProcessedSubscriptions = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("EventHandlerRegistrationService.StartAsync started");

        try
        {
            var subscriptionsManager = serviceProvider.GetRequiredService<IMessageBusSubscriptionsManager>();
            var registrationsList = registrations.ToList();

            Console.WriteLine($"Total registrations to process: {registrationsList.Count}");

            // ✅ Group by event type to avoid duplicates
            var groupedRegistrations = registrationsList
                .GroupBy(r => $"{r.EventType.FullName}|{r.HandlerType.FullName}")
                .ToList();

            Console.WriteLine($"Unique registrations after grouping: {groupedRegistrations.Count}");

            foreach (var group in groupedRegistrations)
            {
                var registration = group.First(); // Take first from group
                var subscriptionKey = $"{registration.EventType.FullName}|{registration.HandlerType.FullName}";

                // ✅ Skip if already processed
                if (ProcessedSubscriptions.Contains(subscriptionKey))
                {
                    Console.WriteLine($"Skipping already processed: {registration.EventType.Name} -> {registration.HandlerType.Name}");
                    continue;
                }

                Console.WriteLine($"Processing: {registration.EventType.Name} -> {registration.HandlerType.Name}");

                try
                {
                    var addSubscriptionMethod = typeof(IMessageBusSubscriptionsManager)
                        .GetMethod("AddSubscription")
                        ?.MakeGenericMethod(registration.EventType, registration.HandlerType);

                    if (addSubscriptionMethod == null)
                    {
                        Console.WriteLine($"AddSubscription method not found for {registration.EventType.Name}");
                        continue;
                    }

                    addSubscriptionMethod.Invoke(subscriptionsManager, Array.Empty<object>());

                    // ✅ Mark as processed
                    ProcessedSubscriptions.Add(subscriptionKey);

                    Console.WriteLine($"Successfully registered: {registration.EventType.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error registering {registration.EventType.Name}: {ex.Message}");
                    if (ex.InnerException != null)
                        Console.WriteLine($"   Inner: {ex.InnerException.Message}");

                    // ✅ Don't mark as processed if failed
                }
            }

            Console.WriteLine("EventHandlerRegistrationService.StartAsync completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General error in StartAsync: {ex.Message}");
            throw;
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Clear processed subscriptions (for testing purposes)
    /// </summary>
    public static void ClearProcessedSubscriptions()
    {
        ProcessedSubscriptions.Clear();
    }
}