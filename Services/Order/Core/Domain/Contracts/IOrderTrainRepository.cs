using BuildingBlocks.Contracts;
using Order.Domain.Entities;

namespace Order.Domain.Contracts;

public interface IOrderTrainRepository : IRepositoryBase<OrderTrain, long>
{
    Task<IEnumerable<OrderTrain>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<OrderTrain?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default);
}