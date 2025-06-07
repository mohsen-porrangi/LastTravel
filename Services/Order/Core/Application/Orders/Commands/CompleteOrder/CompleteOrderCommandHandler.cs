using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Order.Domain.Contracts;
using Order.Domain.Enums;

namespace Order.Application.Orders.Commands.CompleteOrder;

public class CompleteOrderCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<CompleteOrderCommandHandler> logger) : ICommandHandler<CompleteOrderCommand>
{
    public async Task<Unit> Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Orders.FindWithIncludesAsync(o => o.Id == request.OrderId,
                include: q => q.Include(o => o.OrderFlights).Include(o => o.OrderTrains).Include(o => o.OrderTrainCarTransports),
                track: true)
           .ContinueWith(t => t.Result.FirstOrDefault(), cancellationToken)
            ?? throw new NotFoundException("سفارش یافت نشد");

        if (order.LastStatus != OrderStatus.Processing)
            throw new BadRequestException("سفارش در وضعیت مناسب برای تکمیل نیست");

        // بررسی صدور همه بلیط‌ها
        var allTicketsIssued = CheckAllTicketsIssued(order);

        if (!allTicketsIssued)
            throw new BadRequestException("همه بلیط‌ها هنوز صادر نشده‌اند");

        order.UpdateStatus(OrderStatus.Completed, "همه بلیط‌ها با موفقیت صادر شد");

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Order {OrderId} completed successfully", request.OrderId);

        return Unit.Value;
    }

    private static bool CheckAllTicketsIssued(Domain.Entities.Order order)
    {
        // بررسی پروازها
        var unissuedFlights = order.OrderFlights.Any(f => string.IsNullOrEmpty(f.TicketNumber));

        // بررسی قطارها  
        var unissuedTrains = order.OrderTrains.Any(t => string.IsNullOrEmpty(t.TicketNumber));

        // بررسی حمل خودرو
        var unissuedCarTransports = order.OrderTrainCarTransports.Any(c => string.IsNullOrEmpty(c.TicketNumber));

        return !unissuedFlights && !unissuedTrains && !unissuedCarTransports;
    }
}