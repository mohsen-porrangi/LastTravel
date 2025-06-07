using BuildingBlocks.Contracts;
using Order.Domain.Entities;

namespace Order.Domain.Contracts;

public interface ISavedPassengerRepository : IRepositoryBase<SavedPassenger, long>
{
    Task<IEnumerable<SavedPassenger>> GetUserPassengersAsync(Guid userId, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<SavedPassenger?> GetByNationalCodeAsync(Guid userId, string nationalCode, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, string nationalCode, CancellationToken cancellationToken = default);
}