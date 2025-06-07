using BuildingBlocks.Contracts;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Order.Domain.Contracts;
using Order.Domain.Enums;

namespace Order.Application.Orders.Commands.CancelOrder;

/// <summary>
/// Handler برای لغو سفارش با قوانین کسب‌وکار
/// </summary>
public class CancelOrderCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ILogger<CancelOrderCommandHandler> logger) : ICommandHandler<CancelOrderCommand, CancelOrderResult>
{
    public async Task<CancelOrderResult> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserService.GetCurrentUserId();

        logger.LogInformation("Processing cancel request for order {OrderId} by user {UserId}",
            request.OrderId, currentUserId);

        try
        {
            // ✅ دریافت سفارش با اطلاعات مورد نیاز برای validation
            var order = await unitOfWork.Orders.FirstOrDefaultWithIncludesAsync(
                predicate: o => o.Id == request.OrderId && !o.IsDeleted,
                include: q => q.Include(o => o.OrderFlights).Include(o => o.OrderTrains),
                track: true,
                cancellationToken: cancellationToken);

            if (order == null)
                throw new NotFoundException("سفارش یافت نشد");

            // ✅ بررسی مالکیت
            if (order.UserId != currentUserId)
                throw new ForbiddenDomainException("شما اجازه لغو این سفارش را ندارید");

            // ✅ اعتبارسنجی قوانین لغو
            ValidateCancellationRules(order);

            // ✅ اجرای لغو
            order.UpdateStatus(OrderStatus.Cancelled, request.Reason);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Order {OrderId} successfully cancelled by user {UserId}",
                request.OrderId, currentUserId);

            return new CancelOrderResult(
                OrderId: order.Id,
                OrderNumber: order.OrderNumber,
                CancelledAt: DateTime.UtcNow,
                Reason: request.Reason);
        }
        catch (Exception ex) when (ex is not BadRequestException and not NotFoundException and not ForbiddenDomainException)
        {
            logger.LogError(ex, "Unexpected error while cancelling order {OrderId}", request.OrderId);
            throw new InternalServerException("خطا در لغو سفارش");
        }
    }

    /// <summary>
    /// اعتبارسنجی قوانین کسب‌وکار لغو
    /// </summary>
    private static void ValidateCancellationRules(Domain.Entities.Order order)
    {
        // بررسی وضعیت کلی
        if (!order.CanBeCancelled())
            throw new BadRequestException($"سفارش در وضعیت {order.LastStatus} قابل لغو نیست");

        // قوانین خاص برای هر نوع سرویس
        switch (order.ServiceType)
        {
            case ServiceType.DomesticFlight:
            case ServiceType.InternationalFlight:
                ValidateFlightCancellation(order);
                break;
            case ServiceType.Train:
                ValidateTrainCancellation(order);
                break;
        }
    }

    private static void ValidateFlightCancellation(Domain.Entities.Order order)
    {
        var earliestFlight = order.OrderFlights
            .Where(f => f.TicketDirection == TicketDirection.Outbound)
            .OrderBy(f => f.DepartureTime)
            .FirstOrDefault();

        if (earliestFlight != null)
        {
            var timeUntilDeparture = earliestFlight.DepartureTime.Subtract(DateTime.UtcNow);

            if (timeUntilDeparture.TotalHours < 2)
                throw new BadRequestException("لغو پرواز حداقل 2 ساعت قبل از زمان پرواز امکان‌پذیر است");

            // بررسی صدور بلیط
            if (!string.IsNullOrEmpty(earliestFlight.TicketNumber))
                throw new BadRequestException("پس از صدور بلیط، لغو پرواز نیاز به تماس با پشتیبانی دارد");
        }
    }

    private static void ValidateTrainCancellation(Domain.Entities.Order order)
    {
        var earliestTrain = order.OrderTrains
            .Where(t => t.TicketDirection == TicketDirection.Outbound)
            .OrderBy(t => t.DepartureTime)
            .FirstOrDefault();

        if (earliestTrain != null)
        {
            var timeUntilDeparture = earliestTrain.DepartureTime.Subtract(DateTime.UtcNow);

            if (timeUntilDeparture.TotalHours < 1)
                throw new BadRequestException("لغو قطار حداقل 1 ساعت قبل از زمان حرکت امکان‌پذیر است");

            // بررسی صدور بلیط
            if (!string.IsNullOrEmpty(earliestTrain.TicketNumber))
                throw new BadRequestException("پس از صدور بلیط، لغو قطار نیاز به تماس با پشتیبانی دارد");
        }
    }
}