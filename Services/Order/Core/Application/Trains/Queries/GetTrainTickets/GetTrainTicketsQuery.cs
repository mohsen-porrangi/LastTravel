using BuildingBlocks.CQRS;
using Order.Application.Common.DTOs;

namespace Order.Application.Trains.Queries.GetTrainTickets;

/// <summary>
/// Query برای دریافت بلیط‌های قطار یک سفارش
/// </summary>
public record GetTrainTicketsQuery(Guid OrderId) : IQuery<GetTrainTicketsResult>;

/// <summary>
/// نتیجه دریافت بلیط‌های قطار
/// </summary>
public record GetTrainTicketsResult
{
    public List<TrainTicketDto> Tickets { get; init; } = new();
    public string OrderNumber { get; init; } = string.Empty;
    public int TotalTickets { get; init; }
    public int IssuedTickets { get; init; }
    public bool AllTicketsIssued { get; init; }
}