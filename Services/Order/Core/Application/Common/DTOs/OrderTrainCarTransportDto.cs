using Order.Domain.Enums;

namespace Order.Application.Common.DTOs;

/// <summary>
/// DTO برای حمل خودرو با قطار
/// </summary>
public record OrderTrainCarTransportDto : OrderItemDto
{
    /// <summary>
    /// شماره پلاک خودرو
    /// </summary>
    public string CarNumber { get; init; } = string.Empty;

    /// <summary>
    /// نام/مدل خودرو
    /// </summary>
    public string CarName { get; init; } = string.Empty;

    /// <summary>
    /// هزینه حمل خودرو
    /// </summary>
    public decimal TransportAmount { get; init; }

    public OrderTrainCarTransportDto(
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
        string carNumber,
        string carName,
        decimal transportAmount)
        : base(id, passengerNameEn, passengerNameFa, sourceName, destinationName,
               departureTime, arrivalTime, ticketNumber, pnr, totalPrice, direction)
    {
        CarNumber = carNumber;
        CarName = carName;
        TransportAmount = transportAmount;
    }
}