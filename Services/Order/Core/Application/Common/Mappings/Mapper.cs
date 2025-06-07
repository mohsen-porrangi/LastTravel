using AutoMapper;
using Order.Application.Common.DTOs;

namespace Order.Application.Common.Mappings
{
    public static class Mapper
    {
        public static List<OrderItemDto> MapOrderItems(this IMapper mapper, Order.Domain.Entities.Order order)
        {
            var items = new List<OrderItemDto>();
            items.AddRange(mapper.Map<List<OrderFlightDto>>(order.OrderFlights));
            items.AddRange(mapper.Map<List<OrderTrainDto>>(order.OrderTrains));
            return items;
        }
    }
}
