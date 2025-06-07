using BuildingBlocks.Data;
using Microsoft.EntityFrameworkCore;
using Order.Domain.Contracts;
using Order.Infrastructure.Data.Context;

namespace Order.Infrastructure.Data.Repositories;

public class OrderRepository(OrderDbContext context)
    : RepositoryBase<Domain.Entities.Order, Guid, OrderDbContext>(context), IOrderRepository
{
    public async Task<Domain.Entities.Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.OrderFlights)
            .Include(o => o.OrderTrains)
            .Include(o => o.OrderTrainCarTransports)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
        //TODO dynamic include
    }

    public async Task<IEnumerable<Domain.Entities.Order>> GetUserOrdersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasUnissuedFlightsAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await context.OrderFlights
     .AnyAsync(f => f.OrderId == orderId && string.IsNullOrEmpty(f.TicketNumber), cancellationToken);
    }

    public async Task<bool> HasUnissuedTrainAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await context.OrderTrains
     .AnyAsync(f => f.OrderId == orderId && string.IsNullOrEmpty(f.TicketNumber), cancellationToken);
    }

    public async Task LoadStatusHistoriesAsync(Domain.Entities.Order order, CancellationToken cancellationToken)
    {
        if (!context.Entry(order).Collection(o => o.StatusHistories).IsLoaded)
        {
            await context.Entry(order)
                .Collection(o => o.StatusHistories)
                .LoadAsync(cancellationToken);
        }
    }

    public async Task LoadWalletTransactionsAsync(Domain.Entities.Order order, CancellationToken cancellationToken)
    {
        if (!context.Entry(order).Collection(o => o.WalletTransactions).IsLoaded)
        {
            await context.Entry(order)
                .Collection(o => o.WalletTransactions)
                .LoadAsync(cancellationToken);
        }
    }

    public async Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }
}