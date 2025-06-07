using Order.Domain.Enums;

namespace Order.Application.Common.DTOs;

/// <summary>
/// اطلاعات بلیط پرواز
/// </summary>
public record FlightTicketDto
{
    public long Id { get; init; }
    public string PassengerNameEn { get; init; } = string.Empty;
    public string PassengerNameFa { get; init; } = string.Empty;
    public string FlightNumber { get; init; } = string.Empty;
    public FlightProvider Provider { get; init; }
    public string ProviderName { get; init; } = string.Empty;
    public string SourceName { get; init; } = string.Empty;
    public string DestinationName { get; init; } = string.Empty;
    public DateTime DepartureTime { get; init; }
    public DateTime ArrivalTime { get; init; }
    public TicketDirection Direction { get; init; }
    public string DirectionName { get; init; } = string.Empty;
    public string? TicketNumber { get; init; }
    public string? PNR { get; init; }
    public string? SeatNumber { get; init; }
    public DateTime? IssueDate { get; init; }
    public decimal TotalPrice { get; init; }
    public AgeGroup AgeGroup { get; init; }
    public string AgeGroupName { get; init; } = string.Empty;
}