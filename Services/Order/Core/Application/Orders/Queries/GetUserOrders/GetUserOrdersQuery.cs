using BuildingBlocks.CQRS;
using Order.Application.Common.DTOs;
using Order.Application.Common.Models;
using Order.Domain.Enums;

namespace Order.Application.Orders.Queries.GetUserOrders;

/// <summary>
/// Query برای دریافت لیست سفارشات کاربر با قابلیت‌های پیشرفته
/// </summary>
public record GetUserOrdersQuery : IQuery<PaginatedList<OrderSummaryDto>>
{
    /// <summary>
    /// شماره صفحه (پیش‌فرض: 1)
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// تعداد آیتم در هر صفحه (پیش‌فرض: 10، حداکثر: 100)
    /// </summary>
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// فیلتر بر اساس وضعیت سفارش
    /// </summary>
    public OrderStatus? Status { get; init; }

    /// <summary>
    /// فیلتر بر اساس نوع سرویس
    /// </summary>
    public ServiceType? ServiceType { get; init; }

    /// <summary>
    /// تاریخ شروع بازه جستجو
    /// </summary>
    public DateTime? FromDate { get; init; }

    /// <summary>
    /// تاریخ پایان بازه جستجو
    /// </summary>
    public DateTime? ToDate { get; init; }

    /// <summary>
    /// عبارت جستجو در شماره سفارش
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// فیلد مرتب‌سازی (createdAt, orderNumber, amount, status)
    /// </summary>
    public string? SortBy { get; init; }

    /// <summary>
    /// جهت مرتب‌سازی (asc, desc) - پیش‌فرض: desc
    /// </summary>
    public string SortDirection { get; init; } = "desc";
}