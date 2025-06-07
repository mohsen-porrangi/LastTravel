using Microsoft.EntityFrameworkCore;
using Order.Domain.Services;
using Order.Infrastructure.Data.Context;

namespace Order.Infrastructure.Services;

public class OrderNumberGenerator(OrderDbContext context) : IOrderNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"ORD-{date}-";

        // Get the last order number for today
        var lastOrder = await context.Orders
           .Where(o => o.OrderNumber.StartsWith(prefix))
           .OrderByDescending(o => o.OrderNumber)
           .FirstOrDefaultAsync(cancellationToken);

        var sequence = 1;
        if (lastOrder != null)
        {
            var lastSequence = lastOrder.OrderNumber.Split('-').Last();
            if (int.TryParse(lastSequence, out var seq))
                sequence = seq + 1;
        }

        return $"{prefix}{sequence:D3}";
    }
}