using AutoMapper;
using BuildingBlocks.Contracts;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Order.Application.Common.DTOs;
using Order.Domain.Contracts;

namespace Order.Application.Orders.Queries.GetOrderDetails;

/// <summary>
/// Handler برای دریافت جزئیات کامل سفارش
/// </summary>
public class GetOrderDetailsQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IMapper mapper,
    ILogger<GetOrderDetailsQueryHandler> logger) : IQueryHandler<GetOrderDetailsQuery, OrderDetailsDto>
{
    public async Task<OrderDetailsDto> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        logger.LogDebug("Getting order details for order {OrderId} and user {UserId}", request.OrderId, userId);

        try
        {
            // ✅ استفاده از Repository جدید با includes
            var order = await unitOfWork.Orders.FirstOrDefaultWithIncludesAsync(
                predicate: o => o.Id == request.OrderId && o.UserId == userId && !o.IsDeleted,
                include: q => q
                    .Include(o => o.OrderFlights)
                    .Include(o => o.OrderTrains)
                    .Include(o => o.OrderTrainCarTransports)
                    .Include(o => o.StatusHistories.OrderByDescending(h => h.CreatedAt))
                    .Include(o => o.WalletTransactions),
                track: false,
                cancellationToken: cancellationToken);

            if (order == null)
                throw new NotFoundException("سفارش یافت نشد یا شما اجازه دسترسی به آن را ندارید");

            var result = mapper.Map<OrderDetailsDto>(order);

            logger.LogDebug("Successfully retrieved order details for {OrderId}", request.OrderId);

            return result;
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
            logger.LogError(ex, "Error retrieving order details for {OrderId}", request.OrderId);
            throw new InternalServerException("خطا در دریافت جزئیات سفارش");
        }
    }
}