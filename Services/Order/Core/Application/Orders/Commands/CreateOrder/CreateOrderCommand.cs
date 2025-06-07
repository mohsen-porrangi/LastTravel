using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using Order.Domain.Enums;

namespace Order.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand : ICommand<CreateOrderResult>
{
    public ServiceType ServiceType { get; init; }
    public int SourceCode { get; init; }
    public int DestinationCode { get; init; }
    public string SourceName { get; init; } = string.Empty;
    public string DestinationName { get; init; } = string.Empty;
    public DateTime DepartureDate { get; init; }
    public DateTime? ReturnDate { get; init; }
    public List<CreateOrderPassengerInfo> Passengers { get; init; } = new();
    //  public List<CreateOrderItemInfo> Items { get; init; } = new();
    public string FlightNumber { get; init; } = string.Empty; // برای پرواز
    public string TrainNumber { get; init; } = string.Empty; // برای قطار
    public int ProviderId { get; init; } // شرکت ارائه‌دهنده
    public decimal BasePrice { get; init; } // قیمت پایه
}

public record CreateOrderPassengerInfo(
    string FirstNameEn,
    string LastNameEn,
    string FirstNameFa,
    string LastNameFa,
    DateTime BirthDate,
    Gender Gender,
    bool IsIranian,
    string? NationalCode,
    string? PassportNumber
);

//public record CreateOrderItemInfo(
//    TicketDirection Direction,
//    DateTime DepartureTime,
//    DateTime ArrivalTime,
//    string ServiceNumber, // Flight or Train number
//    string Provider,
//    decimal BasePrice
//);

public record CreateOrderResult(
    Guid OrderId,
    string OrderNumber,
    decimal TotalAmount
);