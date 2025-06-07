using Order.Domain.Enums;

namespace Order.Application.Common.DTOs;

public record OrderDto(
    Guid Id,
    string OrderNumber,
    ServiceType ServiceType,
    decimal TotalAmount,
    OrderStatus Status,
    int PassengerCount,
    bool HasReturn,
    DateTime CreatedAt,
    List<OrderItemDto> Items
);

//public record OrderSummaryDto
//{
//    public Guid Id { get; init; }
//    public string OrderNumber { get; init; } = string.Empty;
//    public ServiceType ServiceType { get; init; }
//    public decimal FullAmount { get; init; }
//    public OrderStatus LastStatus { get; init; }
//    public DateTime CreatedAt { get; init; }
//}