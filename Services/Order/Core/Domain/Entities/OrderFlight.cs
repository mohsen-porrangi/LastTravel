using BuildingBlocks.Enums;
using Order.Domain.Enums;

namespace Order.Domain.Entities;

public class OrderFlight : OrderItem
{
    public string FlightNumber { get; private set; } = string.Empty;
    public FlightProvider ProviderId { get; private set; }
    public decimal FlightAmount => TotalPrice; // Alias for consistency

    // Navigation
    public virtual Order Order { get; private set; } = null!;

    protected OrderFlight() { }

    public OrderFlight(
        Guid orderId,
        string firstNameEn, string lastNameEn,
        string firstNameFa, string lastNameFa,
        DateTime birthDate, Gender gender,
        bool isIranian, string? nationalCode, string? passportNumber,
        int sourceCode, int destinationCode,
        string sourceName, string destinationName,
        TicketDirection ticketDirection,
        DateTime departureTime, DateTime arrivalTime,
        string flightNumber, FlightProvider providerId)
        : base(orderId, firstNameEn, lastNameEn, firstNameFa, lastNameFa,
               birthDate, gender, isIranian, nationalCode, passportNumber,
               sourceCode, destinationCode, sourceName, destinationName,
               ticketDirection, departureTime, arrivalTime)
    {
        FlightNumber = flightNumber;
        ProviderId = providerId;
    }
}