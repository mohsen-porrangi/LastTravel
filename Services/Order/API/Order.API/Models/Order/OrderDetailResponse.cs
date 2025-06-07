using Order.Domain.Enums;

namespace Order.API.Models.Order;

public record OrderDetailResponse
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public ServiceType ServiceType { get; init; }
    public decimal TotalAmount { get; init; }
    public OrderStatus Status { get; init; }
    public List<OrderItemDetail> Items { get; init; } = new();
    public List<TransactionDetail> Transactions { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}

public record OrderItemDetail
{
    public long Id { get; init; }
    public string PassengerName { get; init; } = string.Empty;
    public string ServiceNumber { get; init; } = string.Empty;
    public TicketDirection Direction { get; init; }
    public DateTime DepartureTime { get; init; }
    public string? TicketNumber { get; init; }
    public string? PNR { get; init; }
    public decimal Price { get; init; }
}

public record TransactionDetail
{
    public long TransactionId { get; init; }
    public TransactionType Type { get; init; }
    public decimal Amount { get; init; }
    public DateTime CreatedAt { get; init; }
}