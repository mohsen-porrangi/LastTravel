using Order.Domain.Enums;

namespace Order.Application.Common.DTOs;

public record OrderFlightDto : OrderItemDto
{
    public string FlightNumber { get; init; }
    public FlightProvider Provider { get; init; }

    public OrderFlightDto(
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
        string flightNumber,
        FlightProvider provider)
        : base(id, passengerNameEn, passengerNameFa, sourceName, destinationName,
               departureTime, arrivalTime, ticketNumber, pnr, totalPrice, direction)
    {
        FlightNumber = flightNumber;
        Provider = provider;
    }
}