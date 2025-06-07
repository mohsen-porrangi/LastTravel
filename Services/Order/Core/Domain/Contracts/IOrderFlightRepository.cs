using BuildingBlocks.Contracts;
using Order.Domain.Entities;

namespace Order.Domain.Contracts;

public interface IOrderFlightRepository : IRepositoryBase<OrderFlight, long>
{
    Task<IEnumerable<OrderFlight>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<OrderFlight?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default);
}