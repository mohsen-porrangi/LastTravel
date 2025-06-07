using AutoMapper;
using BuildingBlocks.Enums;
using Order.Application.Common.DTOs;
using Order.Domain.Entities;
using Order.Domain.Enums;

namespace Order.Application.Common.Mappings;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Domain.Entities.Order, OrderDto>()
            .ForMember(d => d.TotalAmount, opt => opt.MapFrom(s => s.FullAmount))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.LastStatus))
            .ForMember(d => d.Items, opt => opt.Ignore()); // Items جداگانه map می‌شود

        CreateMap<Domain.Entities.Order, OrderSummaryDto>()
            .ForMember(d => d.TotalAmount, opt => opt.MapFrom(s => s.FullAmount))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.LastStatus))
            .ForMember(d => d.ServiceTypeName, opt => opt.MapFrom(s => GetServiceTypeName(s.ServiceType)))
            .ForMember(d => d.StatusName, opt => opt.MapFrom(s => GetStatusName(s.LastStatus)));

        //   mapping برای OrderFlight
        CreateMap<OrderFlight, OrderFlightDto>()
            .ForMember(d => d.PassengerNameEn, opt => opt.MapFrom(s => $"{s.FirstNameEn} {s.LastNameEn}"))
            .ForMember(d => d.PassengerNameFa, opt => opt.MapFrom(s => $"{s.FirstNameFa} {s.LastNameFa}"))
            .ForMember(d => d.SourceName, opt => opt.MapFrom(s => s.SourceName))
            .ForMember(d => d.DestinationName, opt => opt.MapFrom(s => s.DestinationName))
            .ForMember(d => d.DepartureTime, opt => opt.MapFrom(s => s.DepartureTime))
            .ForMember(d => d.ArrivalTime, opt => opt.MapFrom(s => s.ArrivalTime))
            .ForMember(d => d.TicketNumber, opt => opt.MapFrom(s => s.TicketNumber))
            .ForMember(d => d.PNR, opt => opt.MapFrom(s => s.PNR))
            .ForMember(d => d.TotalPrice, opt => opt.MapFrom(s => s.TotalPrice)) //  استفاده از TotalPrice
            .ForMember(d => d.Direction, opt => opt.MapFrom(s => s.TicketDirection))
            .ForMember(d => d.FlightNumber, opt => opt.MapFrom(s => s.FlightNumber))
            .ForMember(d => d.Provider, opt => opt.MapFrom(s => s.ProviderId));

        //   mapping برای OrderTrain
        CreateMap<OrderTrain, OrderTrainDto>()
            .ForMember(d => d.PassengerNameEn, opt => opt.MapFrom(s => $"{s.FirstNameEn} {s.LastNameEn}"))
            .ForMember(d => d.PassengerNameFa, opt => opt.MapFrom(s => $"{s.FirstNameFa} {s.LastNameFa}"))
            .ForMember(d => d.SourceName, opt => opt.MapFrom(s => s.SourceName))
            .ForMember(d => d.DestinationName, opt => opt.MapFrom(s => s.DestinationName))
            .ForMember(d => d.DepartureTime, opt => opt.MapFrom(s => s.DepartureTime))
            .ForMember(d => d.ArrivalTime, opt => opt.MapFrom(s => s.ArrivalTime))
            .ForMember(d => d.TicketNumber, opt => opt.MapFrom(s => s.TicketNumber))
            .ForMember(d => d.PNR, opt => opt.MapFrom(s => s.PNR))
            .ForMember(d => d.TotalPrice, opt => opt.MapFrom(s => s.TotalPrice)) //  استفاده از TotalPrice
            .ForMember(d => d.Direction, opt => opt.MapFrom(s => s.TicketDirection))
            .ForMember(d => d.TrainNumber, opt => opt.MapFrom(s => s.TrainNumber))
            .ForMember(d => d.Provider, opt => opt.MapFrom(s => s.ProviderId));

        //   mapping برای SavedPassenger
        CreateMap<SavedPassenger, PassengerDto>()
            .ForMember(d => d.AgeGroup, opt => opt.MapFrom(s => CalculateAgeGroup(s.BirthDate)))
            .ForMember(d => d.IsIranian, opt => opt.MapFrom(s => !string.IsNullOrEmpty(s.NationalCode)));
        CreateMap<OrderTrainCarTransport, OrderTrainCarTransportDto>()
            .ForMember(d => d.PassengerNameEn, opt => opt.MapFrom(s => $"{s.FirstNameEn} {s.LastNameEn}"))
            .ForMember(d => d.PassengerNameFa, opt => opt.MapFrom(s => $"{s.FirstNameFa} {s.LastNameFa}"))
            .ForMember(d => d.SourceName, opt => opt.MapFrom(s => s.SourceName))
            .ForMember(d => d.DestinationName, opt => opt.MapFrom(s => s.DestinationName))
            .ForMember(d => d.DepartureTime, opt => opt.MapFrom(s => s.DepartureTime))
            .ForMember(d => d.ArrivalTime, opt => opt.MapFrom(s => s.ArrivalTime))
            .ForMember(d => d.TicketNumber, opt => opt.MapFrom(s => s.TicketNumber))
            .ForMember(d => d.PNR, opt => opt.MapFrom(s => s.PNR))
            .ForMember(d => d.TotalPrice, opt => opt.MapFrom(s => s.TotalPrice))
            .ForMember(d => d.Direction, opt => opt.MapFrom(s => s.TicketDirection))
            .ForMember(d => d.TransportAmount, opt => opt.MapFrom(s => s.TransportAmount));


        CreateMap<OrderFlight, FlightTicketDto>()
    .ForMember(d => d.PassengerNameEn, opt => opt.MapFrom(s => $"{s.FirstNameEn} {s.LastNameEn}"))
    .ForMember(d => d.PassengerNameFa, opt => opt.MapFrom(s => $"{s.FirstNameFa} {s.LastNameFa}"))
    .ForMember(d => d.Provider, opt => opt.MapFrom(s => s.ProviderId))
    .ForMember(d => d.ProviderName, opt => opt.MapFrom(s => GetProviderName(s.ProviderId)))
    .ForMember(d => d.Direction, opt => opt.MapFrom(s => s.TicketDirection))
    .ForMember(d => d.DirectionName, opt => opt.MapFrom(s => GetDirectionName(s.TicketDirection)))
    .ForMember(d => d.AgeGroupName, opt => opt.MapFrom(s => GetAgeGroupName(s.AgeGroup)));

        CreateMap<OrderTrain, TrainTicketDto>()
            .ForMember(d => d.PassengerNameEn, opt => opt.MapFrom(s => $"{s.FirstNameEn} {s.LastNameEn}"))
            .ForMember(d => d.PassengerNameFa, opt => opt.MapFrom(s => $"{s.FirstNameFa} {s.LastNameFa}"))
            .ForMember(d => d.Provider, opt => opt.MapFrom(s => s.ProviderId))
            .ForMember(d => d.ProviderName, opt => opt.MapFrom(s => GetTrainProviderName(s.ProviderId)))
            .ForMember(d => d.Direction, opt => opt.MapFrom(s => s.TicketDirection))
            .ForMember(d => d.DirectionName, opt => opt.MapFrom(s => GetDirectionName(s.TicketDirection)))
            .ForMember(d => d.AgeGroupName, opt => opt.MapFrom(s => GetAgeGroupName(s.AgeGroup)));
        CreateMap<Domain.Entities.Order, OrderDetailsDto>()
      .ForMember(d => d.TotalAmount, opt => opt.MapFrom(s => s.FullAmount))
      .ForMember(d => d.Status, opt => opt.MapFrom(s => s.LastStatus))
      .ForMember(d => d.ServiceTypeName, opt => opt.MapFrom(s => GetServiceTypeName(s.ServiceType)))
      .ForMember(d => d.StatusName, opt => opt.MapFrom(s => GetStatusName(s.LastStatus)))
      .ForMember(d => d.Tickets, opt => opt.MapFrom(s => MapAllTickets(s)))
      .ForMember(d => d.StatusHistory, opt => opt.MapFrom(s => s.StatusHistories))
      .ForMember(d => d.Transactions, opt => opt.MapFrom(s => s.WalletTransactions));

        CreateMap<OrderStatusHistory, StatusHistoryDto>()
            .ForMember(d => d.FromStatusName, opt => opt.MapFrom(s => GetStatusName(s.FromStatus)))
            .ForMember(d => d.ToStatusName, opt => opt.MapFrom(s => GetStatusName(s.ToStatus)));

        CreateMap<OrderWalletTransaction, WalletTransactionDto>()
            .ForMember(d => d.TypeName, opt => opt.MapFrom(s => GetTransactionTypeName(s.Type)));

        // Ticket mappings
        CreateMap<OrderFlight, TicketDetailsDto>()
            .ForMember(d => d.PassengerNameFa, opt => opt.MapFrom(s => $"{s.FirstNameFa} {s.LastNameFa}"))
            .ForMember(d => d.PassengerNameEn, opt => opt.MapFrom(s => $"{s.FirstNameEn} {s.LastNameEn}"))
            .ForMember(d => d.ServiceNumber, opt => opt.MapFrom(s => s.FlightNumber))
            .ForMember(d => d.Direction, opt => opt.MapFrom(s => s.TicketDirection))
            .ForMember(d => d.DirectionName, opt => opt.MapFrom(s => GetDirectionName(s.TicketDirection)))
            .ForMember(d => d.TicketType, opt => opt.MapFrom(s => "پرواز"));

        CreateMap<OrderTrain, TicketDetailsDto>()
            .ForMember(d => d.PassengerNameFa, opt => opt.MapFrom(s => $"{s.FirstNameFa} {s.LastNameFa}"))
            .ForMember(d => d.PassengerNameEn, opt => opt.MapFrom(s => $"{s.FirstNameEn} {s.LastNameEn}"))
            .ForMember(d => d.ServiceNumber, opt => opt.MapFrom(s => s.TrainNumber))
            .ForMember(d => d.Direction, opt => opt.MapFrom(s => s.TicketDirection))
            .ForMember(d => d.DirectionName, opt => opt.MapFrom(s => GetDirectionName(s.TicketDirection)))
            .ForMember(d => d.TicketType, opt => opt.MapFrom(s => "قطار"));
        CreateMap<SavedPassenger, SavedPassengerDto>()
    .ForMember(d => d.FullNameFa, opt => opt.MapFrom(s => $"{s.FirstNameFa} {s.LastNameFa}"))
    .ForMember(d => d.FullNameEn, opt => opt.MapFrom(s => $"{s.FirstNameEn} {s.LastNameEn}"))
    .ForMember(d => d.GenderName, opt => opt.MapFrom(s => GetGenderName(s.Gender)))
    .ForMember(d => d.AgeGroup, opt => opt.MapFrom(s => CalculateAgeGroup(s.BirthDate)))
    .ForMember(d => d.AgeGroupName, opt => opt.MapFrom(s => GetAgeGroupName(CalculateAgeGroup(s.BirthDate))))
    .ForMember(d => d.Age, opt => opt.MapFrom(s => CalculateAge(s.BirthDate)))
    .ForMember(d => d.IsIranian, opt => opt.MapFrom(s => !string.IsNullOrEmpty(s.NationalCode)));

    }

    private static string GetServiceTypeName(ServiceType type) => type switch
    {
        ServiceType.Train => "قطار",
        ServiceType.DomesticFlight => "پرواز داخلی",
        ServiceType.InternationalFlight => "پرواز خارجی",
        _ => "نامشخص"
    };

    private static string GetStatusName(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "در انتظار",
        OrderStatus.Processing => "در حال پردازش",
        OrderStatus.Completed => "تکمیل شده",
        OrderStatus.Cancelled => "لغو شده",
        OrderStatus.Expired => "منقضی شده",
        _ => "نامشخص"
    };

    //    محاسبه AgeGroup
    private static AgeGroup CalculateAgeGroup(DateTime birthDate)
    {
        var age = DateTime.Today.Year - birthDate.Year;
        if (birthDate.Date > DateTime.Today.AddYears(-age)) age--;

        return age switch
        {
            < 2 => AgeGroup.Infant,
            < 12 => AgeGroup.Child,
            _ => AgeGroup.Adult
        };
    }
    private static string GetProviderName(FlightProvider provider) => provider switch
    {
        FlightProvider.Mahan => "ماهان",
        FlightProvider.IranAir => "ایران ایر",
        FlightProvider.Aseman => "آسمان",
        FlightProvider.Kish => "کیش",
        FlightProvider.Qeshm => "قشم",
        _ => "نامشخص"
    };

    private static string GetTrainProviderName(TrainProvider provider) => provider switch
    {
        TrainProvider.Raja => "راجا",
        TrainProvider.Fadak => "فدک",
        TrainProvider.Safar => "سفر",
        _ => "نامشخص"
    };

    private static string GetDirectionName(TicketDirection direction) => direction switch
    {
        TicketDirection.Outbound => "رفت",
        TicketDirection.Inbound => "برگشت",
        _ => "نامشخص"
    };

    private static string GetAgeGroupName(AgeGroup ageGroup) => ageGroup switch
    {
        AgeGroup.Adult => "بزرگسال",
        AgeGroup.Child => "کودک",
        AgeGroup.Infant => "نوزاد",
        _ => "نامشخص"
    };
    private static List<TicketDetailsDto> MapAllTickets(Domain.Entities.Order order)
    {
        var tickets = new List<TicketDetailsDto>();

        // Map flights
        tickets.AddRange(order.OrderFlights.Select(f => new TicketDetailsDto
        {
            Id = f.Id,
            PassengerNameFa = $"{f.FirstNameFa} {f.LastNameFa}",
            PassengerNameEn = $"{f.FirstNameEn} {f.LastNameEn}",
            ServiceNumber = f.FlightNumber,
            Direction = f.TicketDirection,
            DirectionName = GetDirectionName(f.TicketDirection),
            DepartureTime = f.DepartureTime,
            ArrivalTime = f.ArrivalTime,
            SourceName = f.SourceName,
            DestinationName = f.DestinationName,
            TicketNumber = f.TicketNumber,
            PNR = f.PNR,
            SeatNumber = f.SeatNumber,
            IssueDate = f.IssueDate,
            BasePrice = f.BasePrice,
            Tax = f.Tax,
            Fee = f.Fee,
            TotalPrice = f.TotalPrice,
            TicketType = "پرواز"
        }));

        // Map trains
        tickets.AddRange(order.OrderTrains.Select(t => new TicketDetailsDto
        {
            Id = t.Id,
            PassengerNameFa = $"{t.FirstNameFa} {t.LastNameFa}",
            PassengerNameEn = $"{t.FirstNameEn} {t.LastNameEn}",
            ServiceNumber = t.TrainNumber,
            Direction = t.TicketDirection,
            DirectionName = GetDirectionName(t.TicketDirection),
            DepartureTime = t.DepartureTime,
            ArrivalTime = t.ArrivalTime,
            SourceName = t.SourceName,
            DestinationName = t.DestinationName,
            TicketNumber = t.TicketNumber,
            PNR = t.PNR,
            SeatNumber = t.SeatNumber,
            IssueDate = t.IssueDate,
            BasePrice = t.BasePrice,
            Tax = t.Tax,
            Fee = t.Fee,
            TotalPrice = t.TotalPrice,
            TicketType = "قطار"
        }));

        return tickets.OrderBy(t => t.DepartureTime).ToList();
    }
    private static string GetTransactionTypeName(Domain.Enums.TransactionType type) => type switch
    {
        Domain.Enums.TransactionType.Purchase => "خرید",
        Domain.Enums.TransactionType.Refund => "استرداد",
        _ => "نامشخص"
    };
    private static string GetGenderName(Gender gender) => gender switch
    {
        Gender.Male => "آقا",
        Gender.Female => "خانم",
        _ => "نامشخص"
    };

    private static int CalculateAge(DateTime birthDate)
    {
        var age = DateTime.Today.Year - birthDate.Year;
        if (birthDate.Date > DateTime.Today.AddYears(-age)) age--;
        return age;
    }
}