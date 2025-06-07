using Microsoft.EntityFrameworkCore.Storage;
using Order.Domain.Contracts;
using Order.Infrastructure.Data.Context;

namespace Order.Infrastructure.Data.Repositories;

public class UnitOfWork(
    OrderDbContext context,
    IOrderRepository orders,
    IOrderFlightRepository orderFlights,
    IOrderTrainRepository orderTrains,
    ISavedPassengerRepository savedPassengers,
    IOrderTrainCarTransportRepository trainCarTransports
) : IUnitOfWork, IDisposable
{
    private IDbContextTransaction? _transaction;

    public IOrderRepository Orders { get; } = orders;
    public IOrderFlightRepository OrderFlights { get; } = orderFlights;
    public IOrderTrainRepository OrderTrains { get; } = orderTrains;
    public ISavedPassengerRepository SavedPassengers { get; } = savedPassengers;
    public IOrderTrainCarTransportRepository TrainCarTransports { get; } = trainCarTransports;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            _transaction = await context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            if (_transaction != null)
                await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            DisposeTransaction();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            DisposeTransaction();
        }
    }

    private void DisposeTransaction()
    {
        _transaction?.Dispose();
        _transaction = null;
    }

    public void Dispose()
    {
        DisposeTransaction();
        context.Dispose();
    }
}
