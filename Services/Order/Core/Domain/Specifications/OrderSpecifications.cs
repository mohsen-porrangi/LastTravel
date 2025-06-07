using Order.Domain.Enums;
using System.Linq.Expressions;

namespace Order.Domain.Specifications;

public static class OrderSpecifications
{
    public static Expression<Func<Entities.Order, bool>> ByUser(Guid userId)
        => order => order.UserId == userId;

    public static Expression<Func<Entities.Order, bool>> ByStatus(OrderStatus status)
        => order => order.LastStatus == status;

    public static Expression<Func<Entities.Order, bool>> Active()
        => order => order.LastStatus != OrderStatus.Cancelled &&
                   order.LastStatus != OrderStatus.Expired;

    public static Expression<Func<Entities.Order, bool>> Expired(DateTime cutoffTime)
        => order => order.LastStatus == OrderStatus.Pending &&
                   order.CreatedAt < cutoffTime;

    public static Expression<Func<Entities.Order, bool>> ByServiceType(ServiceType serviceType)
        => order => order.ServiceType == serviceType;

    public static Expression<Func<Entities.Order, bool>> ByOrderNumber(string orderNumber)
        => order => order.OrderNumber == orderNumber;
}