using BuildingBlocks.Contracts;

namespace Order.Domain.Contracts;

public interface IOrderRepository : IRepositoryBase<Entities.Order, Guid>
{
    Task<Entities.Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Entities.Order>> GetUserOrdersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<bool> HasUnissuedFlightsAsync(Guid orderId, CancellationToken cancellationToken);
    Task<bool> HasUnissuedTrainAsync(Guid orderId, CancellationToken cancellationToken);
    Task LoadWalletTransactionsAsync(Entities.Order order, CancellationToken cancellationToken);
    Task LoadStatusHistoriesAsync(Entities.Order order, CancellationToken cancellationToken);

}