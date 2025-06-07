using Microsoft.EntityFrameworkCore;
using Order.Domain.Entities;

namespace Order.Application.Common.Interfaces;

public interface IOrderDbContext
{
    DbSet<Domain.Entities.Order> Orders { get; }
    DbSet<OrderFlight> OrderFlights { get; }
    DbSet<OrderTrain> OrderTrains { get; }
    DbSet<OrderTrainCarTransport> OrderTrainCarTransports { get; }
    DbSet<OrderStatusHistory> OrderStatusHistories { get; }
    DbSet<OrderWalletTransaction> OrderWalletTransactions { get; }
    DbSet<SavedPassenger> SavedPassengers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}