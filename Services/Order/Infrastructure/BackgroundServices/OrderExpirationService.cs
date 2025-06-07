using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Order.Application.Common.Interfaces;
using Order.Domain.Enums;

namespace Order.Infrastructure.BackgroundServices;

public class OrderExpirationService(
    IServiceProvider serviceProvider,
    ILogger<OrderExpirationService> logger) : BackgroundService
{
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(30);
    private readonly TimeSpan _expirationTime = TimeSpan.FromHours(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndExpireOrdersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in OrderExpirationService");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckAndExpireOrdersAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IOrderDbContext>();

        var cutoffTime = DateTime.UtcNow.Subtract(_expirationTime);

        var expiredOrders = await context.Orders
            .Where(o => o.LastStatus == OrderStatus.Pending && o.CreatedAt < cutoffTime)
            .ToListAsync(cancellationToken);

        if (expiredOrders.Any())
        {
            logger.LogInformation("Found {Count} expired orders", expiredOrders.Count);

            foreach (var order in expiredOrders)
            {
                order.UpdateStatus(OrderStatus.Expired, "Automatically expired due to timeout");
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}