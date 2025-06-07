using AutoMapper;
using Order.Application.Common.DTOs;

namespace Order.Application.Common.Extensions;

/// <summary>
/// Extension methods برای mapping Order entities
/// </summary>
public static class OrderMappingExtensions
{
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