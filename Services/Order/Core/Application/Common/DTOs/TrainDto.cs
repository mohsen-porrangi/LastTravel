using Order.Domain.Enums;
using Order.Domain.ValueObjects;

namespace Order.Application.Common.DTOs;

public record OrderTrainDto : OrderItemDto
{
    public string TrainNumber { get; init; }
    public TrainProvider Provider { get; init; }

    public OrderTrainDto(
        long id,
        string passengerNameEn,
        string passengerNameFa,
        string sourceName,
        string destinationName,
        DateTime departureTime,
        DateTime arrivalTime,
        string? ticketNumber,
        string? pnr,
        decimal totalPrice,
        TicketDirection direction,
        string trainNumber,
        TrainProvider provider)
        : base(id, passengerNameEn, passengerNameFa, sourceName, destinationName,
               departureTime, arrivalTime, ticketNumber, pnr, totalPrice, direction)
    {
        TrainNumber = trainNumber;
        Provider = provider;
    }
}
public record TrainSearchRequest(int SourceCode, int DestinationCode, DateTime DepartureDate);
public record TrainSearchResult { public List<TrainInfo> Trains { get; init; } = new(); }
public record TrainInfo
{
    public string TrainNumber { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public DateTime DepartureTime { get; init; }
    public DateTime ArrivalTime { get; init; }
    public decimal BasePrice { get; init; }
}
public record TrainReserveRequest(string TrainNumber, List<PassengerInfo> Passengers);
public record TrainReserveResult { public bool Success { get; init; } public string PNR { get; init; } = string.Empty; }

public record TrainTicketRequest(string PNR);
public record TrainTicketResult
{
    public bool Success { get; init; }
    public string TicketNumber { get; init; } = string.Empty;
    public string PdfUrl { get; init; } = string.Empty;
}