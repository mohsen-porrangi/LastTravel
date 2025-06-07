using BuildingBlocks.CQRS;

namespace Order.Application.Orders.Commands.CancelOrder;

/// <summary>
/// Command برای لغو سفارش
/// </summary>
public record CancelOrderCommand(Guid OrderId, string Reason) : ICommand<CancelOrderResult>;

/// <summary>
/// نتیجه لغو سفارش
/// </summary>
public record CancelOrderResult(
    Guid OrderId,
    string OrderNumber,
    DateTime CancelledAt,
    string Reason
);