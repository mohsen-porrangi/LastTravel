using Order.Domain.Enums;

namespace Order.Application.Common.DTOs
{
    public record OrderSummaryDto(
     Guid Id,
     string OrderNumber,
     ServiceType ServiceType,
     string ServiceTypeName,
     decimal TotalAmount,
     OrderStatus Status,
     string StatusName,
     DateTime CreatedAt,
     int PassengerCount,
     bool HasReturn
 );
}
