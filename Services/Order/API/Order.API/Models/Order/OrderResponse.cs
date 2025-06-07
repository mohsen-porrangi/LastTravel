using Order.Domain.Enums;

namespace Order.API.Models.Order;

public record OrderResponse
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public ServiceType ServiceType { get; init; }
    public decimal TotalAmount { get; init; }
    public OrderStatus Status { get; init; }
    public int PassengerCount { get; init; }
    public bool HasReturn { get; init; }
    public DateTime CreatedAt { get; init; }
}