using AutoMapper;
using BuildingBlocks.Contracts;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.Extensions.Logging;
using Order.Application.Common.DTOs;
using Order.Domain.Contracts;

namespace Order.Application.Trains.Queries.GetTrainTickets;

/// <summary>
/// Handler برای دریافت بلیط‌های قطار سفارش
/// </summary>
public class GetTrainTicketsQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IMapper mapper,
    ILogger<GetTrainTicketsQueryHandler> logger) : IQueryHandler<GetTrainTicketsQuery, GetTrainTicketsResult>
{
    public async Task<GetTrainTicketsResult> Handle(GetTrainTicketsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        logger.LogDebug("Fetching train tickets for order {OrderId} by user {UserId}", request.OrderId, userId);

        // ✅ بررسی مالکیت سفارش
        var order = await unitOfWork.Orders.FirstOrDefaultAsync(
            o => o.Id == request.OrderId && o.UserId == userId && !o.IsDeleted,
            track: false,
            cancellationToken);

        if (order == null)
            throw new NotFoundException("سفارش یافت نشد یا شما اجازه دسترسی به آن را ندارید");

        // ✅ دریافت بلیط‌های قطار
        var trainTickets = await unitOfWork.OrderTrains.FindAsync(
            t => t.OrderId == request.OrderId && !t.IsDeleted,
            track: false,
            cancellationToken);

        var trainList = trainTickets.ToList();
        var issuedTickets = trainList.Where(t => !string.IsNullOrEmpty(t.TicketNumber)).ToList();

        var ticketDtos = mapper.Map<List<TrainTicketDto>>(issuedTickets);

        logger.LogDebug("Retrieved {IssuedCount} issued train tickets out of {TotalCount} for order {OrderId}",
            issuedTickets.Count, trainList.Count, request.OrderId);

        return new GetTrainTicketsResult
        {
            Tickets = ticketDtos,
            OrderNumber = order.OrderNumber,
            TotalTickets = trainList.Count,
            IssuedTickets = issuedTickets.Count,
            AllTicketsIssued = trainList.Count > 0 && trainList.All(t => !string.IsNullOrEmpty(t.TicketNumber))
        };
    }
}