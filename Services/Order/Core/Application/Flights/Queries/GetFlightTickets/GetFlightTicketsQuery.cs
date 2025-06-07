using BuildingBlocks.CQRS;
using Order.Application.Common.DTOs;

namespace Order.Application.Flights.Queries.GetFlightTickets;

/// <summary>
/// Query برای دریافت بلیط‌های پرواز یک سفارش
/// </summary>
public record GetFlightTicketsQuery(Guid OrderId) : IQuery<GetFlightTicketsResult>;

/// <summary>
/// نتیجه دریافت بلیط‌های پرواز
/// </summary>
public record GetFlightTicketsResult
{
    public List<FlightTicketDto> Tickets { get; init; } = new();
    public string OrderNumber { get; init; } = string.Empty;
    public int TotalTickets { get; init; }
    public int IssuedTickets { get; init; }
    public bool AllTicketsIssued { get; init; }
}