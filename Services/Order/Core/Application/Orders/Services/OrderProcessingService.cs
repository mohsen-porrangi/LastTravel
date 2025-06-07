using AutoMapper;
using BuildingBlocks.Contracts.Services;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Order.Application.Common.DTOs;
using Order.Application.Common.Interfaces;
using Order.Application.EventHandlers;
using Order.Domain.Contracts;
using Order.Domain.Enums;

namespace Order.Application.Orders.Services;
/// <summary>
///  Flight/Train servicesهماهنگی با 
/// </summary>
/// <param name="unitOfWork"></param>
/// <param name="walletPaymentService"></param>
/// <param name="mapper"></param>
/// <param name="logger"></param>
public class OrderProcessingService(
    IUnitOfWork unitOfWork,
    IWalletPaymentService walletPaymentService,
    IMapper mapper,
    ILogger<OrderProcessingService> logger) : IOrderService
{

    public async Task<OrderDto> ProcessOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderWithItemsAsync(orderId, cancellationToken);

        // Process order logic here

        return mapper.Map<OrderDto>(order);
    }
    public async Task<bool> ValidateOrderForPaymentAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderWithItemsAsync(orderId, cancellationToken);

        // Check order status
        if (order.LastStatus != Domain.Enums.OrderStatus.Pending)
            return false;

        // Check all items have pricing
        var hasValidPricing = order.OrderFlights.All(f => f.TotalPrice > 0) &&
                             order.OrderTrains.All(t => t.TotalPrice > 0);

        return hasValidPricing;
    }
    public async Task<decimal> CalculateTotalAmountAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderWithItemsAsync(orderId, cancellationToken);

        var flightTotal = order.OrderFlights.Sum(f => f.TotalPrice);
        var trainTotal = order.OrderTrains.Sum(t => t.TotalPrice);
        var carTotal = order.OrderTrainCarTransports.Sum(c => c.TotalPrice);

        return flightTotal + trainTotal + carTotal;
    }

    private async Task<Domain.Entities.Order> GetOrderWithItemsAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Orders
            .FindWithIncludesAsync(o => o.Id == orderId,
                include: q => q
                    .Include(o => o.OrderFlights)
                    .Include(o => o.OrderTrains)
                    .Include(o => o.OrderTrainCarTransports))
            .ContinueWith(t => t.Result.FirstOrDefault(), cancellationToken)
            ?? throw new NotFoundException("سفارش یافت نشد");

        return order;
    }

    //public async Task<bool> ProcessPaymentAsync(Guid orderId, CancellationToken cancellationToken)
    //{
    //    var order = await unitOfWork.Orders.GetByIdAsync(orderId, track: true, cancellationToken);
    //    if (order == null) return false;

    //    // Request payment through Wallet service
    //    // This is simplified - actual implementation would include proper payment flow
    //    var paymentRequest = new
    //    {
    //        UserId = order.UserId,
    //        Amount = order.FullAmount,
    //        OrderId = orderId,
    //        Description = $"پرداخت سفارش {order.OrderNumber}"
    //    };

    //    // TODO: Implement actual payment through wallet service
    //    // var paymentResult = await walletPaymentService.ProcessPaymentAsync(paymentRequest);

    //    order.UpdateStatus(OrderStatus.Processing);
    //    await unitOfWork.SaveChangesAsync(cancellationToken);

    //    return true;
    //}

    public async Task<Domain.Entities.Order?> IssueTicketsAsync(PaymentCompletedEvent @event, CancellationToken cancellationToken)
    {
        Domain.Entities.Order? order = await unitOfWork.Orders.GetByIdAsync(@event.OrderId, cancellationToken: cancellationToken);
        if (order == null || order.LastStatus != OrderStatus.Processing)
        {
            logger.LogWarning("Order {OrderId} not found", @event.OrderId);
            return null;
        }
        // Update order status
        order.UpdateStatus(OrderStatus.Processing, "Payment completed");
        // Add wallet transaction reference
        order.AddWalletTransaction(@event.TransactionId, TransactionType.Purchase, @event.Amount);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return order;

        // Publish event to start ticket issuing process        
        // TODO: Call external Flight/Train services to issue tickets
        // This would be implemented when integrating with actual services
    }

    public Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CanCancelOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task ProcessExpiredOrdersAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}