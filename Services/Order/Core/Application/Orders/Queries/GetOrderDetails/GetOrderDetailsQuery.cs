using BuildingBlocks.CQRS;
using Order.Application.Common.DTOs;

namespace Order.Application.Orders.Queries.GetOrderDetails;

public record GetOrderDetailsQuery(Guid OrderId) : IQuery<OrderDetailsDto>;

public record OrderDetailsResponse
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string ServiceType { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public List<TicketInfo> Tickets { get; init; } = new();
}

public record TicketInfo
{
    public string PassengerName { get; init; } = string.Empty;
    public string TicketNumber { get; init; } = string.Empty;
    public string PNR { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public DateTime DepartureTime { get; init; }
    public string SeatNumber { get; init; } = string.Empty;
}