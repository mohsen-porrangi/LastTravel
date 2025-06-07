using AutoMapper;
using Order.API.Models.Order;
using Order.Application.Common.DTOs;
using Order.Application.Common.Models;

namespace Order.API.Extensions;

/// <summary>
/// Extension methods برای تبدیل اطلاعات بین لایه‌های مختلف architecture
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// تبدیل GetUserOrdersRequest به GetUserOrdersQuery برای لایه Application
    /// </summary>
    public static Application.Orders.Queries.GetUserOrders.GetUserOrdersQuery ToQuery(this GetUserOrdersRequest request)
    {
        return new Application.Orders.Queries.GetUserOrders.GetUserOrdersQuery
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            Status = request.Status,
            ServiceType = request.ServiceType,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            SearchTerm = request.SearchTerm,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection
        };
    }

    /// <summary>
    /// تبدیل PaginatedList از Application layer به GetUserOrdersResponse برای API
    /// </summary>
    public static GetUserOrdersResponse ToApiResponse(this PaginatedList<OrderSummaryDto> source)
    {
        var orders = source.Items.Select(ToOrderSummaryResponse).ToList();

        return new GetUserOrdersResponse
        {
            Orders = orders,
            Pagination = new PaginationInfo
            {
                PageNumber = source.PageNumber,
                PageSize = source.Items.Count,
                TotalCount = source.TotalCount,
                TotalPages = source.TotalPages,
                HasPreviousPage = source.HasPreviousPage,
                HasNextPage = source.HasNextPage,
                FirstItemOnPage = source.TotalCount == 0 ? 0 : ((source.PageNumber - 1) * source.Items.Count) + 1,
                LastItemOnPage = source.TotalCount == 0 ? 0 : ((source.PageNumber - 1) * source.Items.Count) + source.Items.Count
            },
            Metadata = new ResponseMetadata
            {
                GeneratedAt = DateTime.UtcNow,
                ApiVersion = "v1",
                HasFilters = HasActiveFilters(source),
                ActiveFiltersCount = CountActiveFilters(source)
            }
        };
    }
    public static object ToApiResponse(this OrderDetailsDto source)
    {
        return new
        {
            order = new
            {
                id = source.Id,
                orderNumber = source.OrderNumber,
                serviceType = source.ServiceType,
                serviceTypeName = source.ServiceTypeName,
                totalAmount = source.TotalAmount,
                status = source.Status,
                statusName = source.StatusName,
                passengerCount = source.PassengerCount,
                hasReturn = source.HasReturn,
                createdAt = source.CreatedAt
            },
            tickets = source.Tickets.Select(t => new
            {
                id = t.Id,
                passengerName = t.PassengerNameFa,
                serviceNumber = t.ServiceNumber,
                direction = t.DirectionName,
                departureTime = t.DepartureTime,
                arrivalTime = t.ArrivalTime,
                route = $"{t.SourceName} ← {t.DestinationName}",
                ticketNumber = t.TicketNumber,
                pnr = t.PNR,
                seatNumber = t.SeatNumber,
                totalPrice = t.TotalPrice,
                ticketType = t.TicketType
            }),
            statusHistory = source.StatusHistory.Select(h => new
            {
                fromStatus = h.FromStatusName,
                toStatus = h.ToStatusName,
                reason = h.Reason,
                changedAt = h.CreatedAt
            }),
            transactions = source.Transactions.Select(t => new
            {
                transactionId = t.TransactionId,
                type = t.TypeName,
                amount = t.Amount,
                createdAt = t.CreatedAt
            })
        };
    }
    public static object ToSavedPassengersResponse(this List<SavedPassengerDto> source)
    {
        return new
        {
            passengers = source.Select(p => new
            {
                id = p.Id,
                fullNameFa = p.FullNameFa,
                fullNameEn = p.FullNameEn,
                nationalCode = p.NationalCode,
                passportNumber = p.PassportNumber,
                age = p.Age,
                ageGroup = p.AgeGroupName,
                gender = p.GenderName,
                isIranian = p.IsIranian
            }),
            totalCount = source.Count
        };
    }
    /// <summary>
    /// تبدیل OrderSummaryDto به OrderSummaryResponse برای API response
    /// </summary>
    public static OrderSummaryResponse ToOrderSummaryResponse(this OrderSummaryDto source)
    {
        return new OrderSummaryResponse
        {
            Id = source.Id,
            OrderNumber = source.OrderNumber,
            ServiceType = source.ServiceType,
            ServiceTypeName = source.ServiceTypeName,
            TotalAmount = source.TotalAmount,
            Status = source.Status,
            StatusName = source.StatusName,
            PassengerCount = source.PassengerCount,
            HasReturn = source.HasReturn,
            CreatedAt = source.CreatedAt,
            Route = ExtractRouteInfo(source)
        };
    }

    /// <summary>
    /// استخراج اطلاعات مسیر از OrderSummaryDto (اگر موجود باشد)
    /// </summary>
    private static RouteInfo? ExtractRouteInfo(OrderSummaryDto source)
    {
        // در آینده می‌توان اطلاعات مسیر را از OrderSummaryDto استخراج کرد
        // فعلاً null برمی‌گردانیم زیرا این اطلاعات در DTO موجود نیست
        return null;
    }

    /// <summary>
    /// بررسی وجود فیلترهای فعال در result
    /// </summary>
    private static bool HasActiveFilters(PaginatedList<OrderSummaryDto> source)
    {
        // این اطلاعات باید از query object یا context دریافت شود
        // فعلاً false برمی‌گردانیم
        return false;
    }

    /// <summary>
    /// شمارش فیلترهای فعال
    /// </summary>
    private static int CountActiveFilters(PaginatedList<OrderSummaryDto> source)
    {
        // این اطلاعات باید از query object یا context دریافت شود
        // فعلاً 0 برمی‌گردانیم
        return 0;
    }
    /// <summary>
    /// تبدیل Order Items به DTOs
    /// </summary>
    public static List<OrderItemDto> MapToOrderItems(this Domain.Entities.Order order, IMapper mapper)
    {
        var items = new List<OrderItemDto>();

        // Map OrderFlights
        if (order.OrderFlights?.Any() == true)
        {
            items.AddRange(mapper.Map<List<OrderFlightDto>>(order.OrderFlights));
        }

        // Map OrderTrains
        if (order.OrderTrains?.Any() == true)
        {
            items.AddRange(mapper.Map<List<OrderTrainDto>>(order.OrderTrains));
        }

        // Map OrderTrainCarTransports
        if (order.OrderTrainCarTransports?.Any() == true)
        {
            items.AddRange(mapper.Map<List<OrderTrainCarTransportDto>>(order.OrderTrainCarTransports));
        }

        return items.OrderBy(i => i.DepartureTime).ToList();
    }

    /// <summary>
    /// Map OrderFlight entities to DTOs
    /// </summary>
    public static List<OrderFlightDto> MapOrderFlights(this ICollection<Domain.Entities.OrderFlight> flights, IMapper mapper)
    {
        return mapper.Map<List<OrderFlightDto>>(flights);
    }

    /// <summary>
    /// Map OrderTrain entities to DTOs
    /// </summary>
    public static List<OrderTrainDto> MapOrderTrains(this ICollection<Domain.Entities.OrderTrain> trains, IMapper mapper)
    {
        return mapper.Map<List<OrderTrainDto>>(trains);
    }
}