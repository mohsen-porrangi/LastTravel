using BuildingBlocks.Data;
using Microsoft.EntityFrameworkCore;
using Order.Domain.Contracts;
using Order.Domain.Entities;
using Order.Infrastructure.Data.Context;

namespace Order.Infrastructure.Data.Repositories;

public class OrderFlightRepository(OrderDbContext context)
    : RepositoryBase<OrderFlight, long, OrderDbContext>(context), IOrderFlightRepository
{
    public async Task<IEnumerable<OrderFlight>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(f => f.OrderId == orderId)
            .OrderBy(f => f.DepartureTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderFlight?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(f => f.TicketNumber == ticketNumber, cancellationToken);
    }
}