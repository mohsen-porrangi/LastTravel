using BuildingBlocks.Enums;
using Order.Domain.Enums;

namespace Order.Domain.Entities;

public class OrderTrain : OrderItem
{
    public string TrainNumber { get; private set; } = string.Empty;
    public TrainProvider ProviderId { get; private set; }
    public decimal TrainAmount => TotalPrice; // Alias for consistency

    // Navigation
    public virtual Order Order { get; private set; } = null!;

    protected OrderTrain() { }

    public OrderTrain(
        Guid orderId,
        string firstNameEn, string lastNameEn,
        string firstNameFa, string lastNameFa,
        DateTime birthDate, Gender gender,
        bool isIranian, string? nationalCode, string? passportNumber,
        int sourceCode, int destinationCode,
        string sourceName, string destinationName,
        TicketDirection ticketDirection,
        DateTime departureTime, DateTime arrivalTime,
        string trainNumber, TrainProvider providerId)
        : base(orderId, firstNameEn, lastNameEn, firstNameFa, lastNameFa,
               birthDate, gender, isIranian, nationalCode, passportNumber,
               sourceCode, destinationCode, sourceName, destinationName,
               ticketDirection, departureTime, arrivalTime)
    {
        TrainNumber = trainNumber;
        ProviderId = providerId;
    }
}