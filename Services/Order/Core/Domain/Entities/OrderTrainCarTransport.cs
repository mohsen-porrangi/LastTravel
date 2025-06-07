using BuildingBlocks.Enums;
using Order.Domain.Enums;

namespace Order.Domain.Entities;

public class OrderTrainCarTransport : OrderItem
{
    public string CarNumber { get; private set; } = string.Empty;
    public string CarName { get; private set; } = string.Empty;
    public decimal TransportAmount => TotalPrice;

    // Navigation
    public virtual Order Order { get; private set; } = null!;

    protected OrderTrainCarTransport() { }

    public OrderTrainCarTransport(
        Guid orderId,
        string firstNameEn, string lastNameEn,
        string firstNameFa, string lastNameFa,
        DateTime birthDate, Gender gender,
        bool isIranian, string? nationalCode, string? passportNumber,
        int sourceCode, int destinationCode,
        string sourceName, string destinationName,
        TicketDirection ticketDirection,
        DateTime departureTime, DateTime arrivalTime,
        string carNumber, string carName)
        : base(orderId, firstNameEn, lastNameEn, firstNameFa, lastNameFa,
               birthDate, gender, isIranian, nationalCode, passportNumber,
               sourceCode, destinationCode, sourceName, destinationName,
               ticketDirection, departureTime, arrivalTime)
    {
        CarNumber = carNumber;
        CarName = carName;
    }
}