using Order.Application.Common.DTOs;

namespace Order.Application.Common.Interfaces;

public interface IOrderService
{
    Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken);
    Task<decimal> CalculateTotalAmountAsync(Guid orderId, CancellationToken cancellationToken);
    Task<bool> CanCancelOrderAsync(Guid orderId, CancellationToken cancellationToken);
    Task ProcessExpiredOrdersAsync(CancellationToken cancellationToken);
    Task<OrderDto> ProcessOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<bool> ValidateOrderForPaymentAsync(Guid orderId, CancellationToken cancellationToken = default);

}