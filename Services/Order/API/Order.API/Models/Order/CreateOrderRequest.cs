using Order.Domain.Enums;

namespace Order.API.Models.Order;

public record CreateOrderRequest
{
    public ServiceType ServiceType { get; init; }
    public int SourceCode { get; init; }
    public int DestinationCode { get; init; }
    public string SourceName { get; init; } = string.Empty;
    public string DestinationName { get; init; } = string.Empty;
    public DateTime DepartureDate { get; init; }
    public DateTime? ReturnDate { get; init; }
    public List<PassengerInfo> Passengers { get; init; } = new();

    // اطلاعات انتخاب شده توسط کاربر
    public string FlightNumber { get; init; } = string.Empty;
    public string TrainNumber { get; init; } = string.Empty;
    public int ProviderId { get; init; }
    public decimal BasePrice { get; init; }
}

public record PassengerInfo
{
    public string FirstNameEn { get; init; } = string.Empty;
    public string LastNameEn { get; init; } = string.Empty;
    public string FirstNameFa { get; init; } = string.Empty;
    public string LastNameFa { get; init; } = string.Empty;
    public DateTime BirthDate { get; init; }
    public BuildingBlocks.Enums.Gender Gender { get; init; }
    public bool IsIranian { get; init; }
    public string? NationalCode { get; init; }
    public string? PassportNumber { get; init; }
}