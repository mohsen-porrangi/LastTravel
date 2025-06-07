using BuildingBlocks.Data;
using Microsoft.EntityFrameworkCore;
using Order.Domain.Contracts;
using Order.Domain.Entities;
using Order.Infrastructure.Data.Context;

namespace Order.Infrastructure.Data.Repositories;

public class SavedPassengerRepository(OrderDbContext context)
    : RepositoryBase<SavedPassenger, long, OrderDbContext>(context), ISavedPassengerRepository
{
    public async Task<IEnumerable<SavedPassenger>> GetUserPassengersAsync(
        Guid userId, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(p => p.UserId == userId);

        if (activeOnly)
            query = query.Where(p => p.IsActive);

        return await query
            .OrderBy(p => p.FirstNameFa)
            .ThenBy(p => p.LastNameFa)
            .ToListAsync(cancellationToken);
    }

    public async Task<SavedPassenger?> GetByNationalCodeAsync(
        Guid userId, string nationalCode, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(p => p.UserId == userId && p.NationalCode == nationalCode, cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid userId, string nationalCode, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(p => p.UserId == userId && p.NationalCode == nationalCode, cancellationToken);
    }
}