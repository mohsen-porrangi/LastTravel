using AutoMapper;
using BuildingBlocks.Contracts;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.Extensions.Logging;
using Order.Application.Common.DTOs;
using Order.Domain.Contracts;

namespace Order.Application.Flights.Queries.GetFlightTickets;

/// <summary>
/// Handler برای دریافت بلیط‌های پرواز سفارش
/// </summary>
public class GetFlightTicketsQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IMapper mapper,
    ILogger<GetFlightTicketsQueryHandler> logger) : IQueryHandler<GetFlightTicketsQuery, GetFlightTicketsResult>
{
    public async Task<GetFlightTicketsResult> Handle(GetFlightTicketsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        logger.LogDebug("Fetching flight tickets for order {OrderId} by user {UserId}", request.OrderId, userId);

        // ✅ بررسی مالکیت سفارش
        var order = await unitOfWork.Orders.FirstOrDefaultAsync(
            o => o.Id == request.OrderId && o.UserId == userId && !o.IsDeleted,
            track: false,
            cancellationToken);

        if (order == null)
            throw new NotFoundException("سفارش یافت نشد یا شما اجازه دسترسی به آن را ندارید");

        // ✅ دریافت بلیط‌های پرواز
        var flightTickets = await unitOfWork.OrderFlights.FindAsync(
            f => f.OrderId == request.OrderId && !f.IsDeleted,
            track: false,
            cancellationToken);

        var flightList = flightTickets.ToList();
        var issuedTickets = flightList.Where(f => !string.IsNullOrEmpty(f.TicketNumber)).ToList();

        var ticketDtos = mapper.Map<List<FlightTicketDto>>(issuedTickets);

        logger.LogDebug("Retrieved {IssuedCount} issued flight tickets out of {TotalCount} for order {OrderId}",
            issuedTickets.Count, flightList.Count, request.OrderId);

        return new GetFlightTicketsResult
        {
            Tickets = ticketDtos,
            OrderNumber = order.OrderNumber,
            TotalTickets = flightList.Count,
            IssuedTickets = issuedTickets.Count,
            AllTicketsIssued = flightList.Count > 0 && flightList.All(f => !string.IsNullOrEmpty(f.TicketNumber))
        };
    }
}