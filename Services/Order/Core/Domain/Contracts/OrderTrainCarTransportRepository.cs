using BuildingBlocks.Contracts;
using Order.Domain.Entities;

namespace Order.Domain.Contracts
{
    public interface IOrderTrainCarTransportRepository : IRepositoryBase<OrderTrainCarTransport, long>
    {
        Task<IEnumerable<OrderTrainCarTransport>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task<OrderTrainCarTransport?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default);
    }
}
