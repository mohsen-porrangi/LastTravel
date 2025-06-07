using AutoMapper;
using BuildingBlocks.Contracts;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.Extensions.Logging;
using Order.Application.Common.DTOs;
using Order.Domain.Contracts;

namespace Order.Application.Passengers.Queries.GetSavedPassengers;

/// <summary>
/// Handler برای دریافت لیست مسافران ذخیره شده کاربر
/// </summary>
public class GetSavedPassengersQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IMapper mapper,
    ILogger<GetSavedPassengersQueryHandler> logger) : IQueryHandler<GetSavedPassengersQuery, List<SavedPassengerDto>>
{
    public async Task<List<SavedPassengerDto>> Handle(GetSavedPassengersQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        logger.LogDebug("Getting saved passengers for user {UserId}", userId);

        try
        {
            // ✅ استفاده از Repository جدید
            var passengers = await unitOfWork.SavedPassengers.FindAsync(
                predicate: p => p.UserId == userId && p.IsActive && !p.IsDeleted,
                track: false,
                cancellationToken: cancellationToken);

            var result = mapper.Map<List<SavedPassengerDto>>(passengers.OrderBy(p => p.FirstNameFa));

            logger.LogDebug("Retrieved {Count} saved passengers for user {UserId}", result.Count, userId);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving saved passengers for user {UserId}", userId);
            throw new InternalServerException("خطا در دریافت لیست مسافران ذخیره شده");
        }
    }
}