using BuildingBlocks.Data;
using Microsoft.EntityFrameworkCore;
using Order.Domain.Contracts;
using Order.Domain.Entities;
using Order.Infrastructure.Data.Context;

namespace Order.Infrastructure.Data.Repositories;

public class OrderTrainRepository(OrderDbContext context)
    : RepositoryBase<OrderTrain, long, OrderDbContext>(context), IOrderTrainRepository
{
    public async Task<IEnumerable<OrderTrain>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.OrderId == orderId)
            .OrderBy(t => t.DepartureTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderTrain?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber, cancellationToken);
    }
}