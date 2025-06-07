using Order.Domain.Enums;

namespace Order.Application.Common.DTOs
{
    public record OrderItemDto(
      long Id,
      string PassengerNameEn,
      string PassengerNameFa,
      string SourceName,
      string DestinationName,
      DateTime DepartureTime,
      DateTime ArrivalTime,
      string? TicketNumber,
      string? PNR,
      decimal TotalPrice,
      TicketDirection Direction
  );
}
