
using BuildingBlocks.CQRS;
using Order.Application.Common.DTOs;

namespace Order.Application.Orders.Queries.GetOrderById;

/// <summary>
/// Query برای دریافت جزئیات کامل یک سفارش خاص
/// </summary>
public record GetOrderByIdQuery(Guid OrderId, IncludeOrder Include = IncludeOrder.All) : IQuery<OrderDto>;

/// <summary>
/// تعیین کننده اطلاعات اضافی که باید include شوند
/// </summary>
[Flags]
public enum IncludeOrder
{
    /// <summary>
    /// هیچ اطلاعات اضافی include نشود
    /// </summary>
    None = 0,

    /// <summary>
    /// اطلاعات پروازها
    /// </summary>
    OrderFlights = 1 << 0,

    /// <summary>
    /// اطلاعات قطارها
    /// </summary>
    OrderTrains = 1 << 1,

    /// <summary>
    /// اطلاعات حمل خودرو با قطار
    /// </summary>
    OrderTrainCarTransports = 1 << 2,

    /// <summary>
    /// تراکنش‌های کیف پول
    /// </summary>
    WalletTransactions = 1 << 3,

    /// <summary>
    /// تاریخچه تغییرات وضعیت
    /// </summary>
    StatusHistory = 1 << 4,

    /// <summary>
    /// فقط آیتم‌های سفارش (پروازها و قطارها)
    /// </summary>
    OrderItems = OrderFlights | OrderTrains | OrderTrainCarTransports,

    /// <summary>
    /// همه اطلاعات اضافی
    /// </summary>
    All = OrderFlights | OrderTrains | OrderTrainCarTransports | WalletTransactions | StatusHistory
}