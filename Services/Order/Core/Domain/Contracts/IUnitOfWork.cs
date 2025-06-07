namespace Order.Domain.Contracts;

public interface IUnitOfWork
{
    IOrderRepository Orders { get; }
    IOrderFlightRepository OrderFlights { get; }
    IOrderTrainRepository OrderTrains { get; }
    ISavedPassengerRepository SavedPassengers { get; }
    IOrderTrainCarTransportRepository TrainCarTransports { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}