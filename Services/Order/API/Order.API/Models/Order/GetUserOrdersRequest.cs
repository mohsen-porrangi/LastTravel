using Order.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Order.API.Models.Order;

/// <summary>
/// مدل درخواست دریافت لیست سفارشات کاربر با تمام قابلیت‌های فیلترینگ و مرتب‌سازی
/// </summary>
public record GetUserOrdersRequest
{
    /// <summary>
    /// شماره صفحه برای صفحه‌بندی (پیش‌فرض: 1، حداقل: 1)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "شماره صفحه باید بزرگتر از 0 باشد")]
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// تعداد آیتم در هر صفحه (پیش‌فرض: 10، حداقل: 1، حداکثر: 100)
    /// </summary>
    [Range(1, 100, ErrorMessage = "اندازه صفحه باید بین 1 تا 100 باشد")]
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// فیلتر بر اساس وضعیت سفارش - امکان فیلتر کردن بر اساس وضعیت‌های مختلف سفارش
    /// </summary>
    public OrderStatus? Status { get; init; }

    /// <summary>
    /// فیلتر بر اساس نوع سرویس - امکان جداسازی سفارشات قطار، پرواز داخلی و بین‌المللی
    /// </summary>
    public ServiceType? ServiceType { get; init; }

    /// <summary>
    /// تاریخ شروع بازه جستجو - برای فیلتر کردن سفارشات از تاریخ مشخص به بعد
    /// </summary>
    public DateTime? FromDate { get; init; }

    /// <summary>
    /// تاریخ پایان بازه جستجو - برای فیلتر کردن سفارشات تا تاریخ مشخص
    /// </summary>
    public DateTime? ToDate { get; init; }

    /// <summary>
    /// عبارت جستجو در شماره سفارش - جستجوی هوشمند با پشتیبانی از جستجوی جزئی و کامل
    /// </summary>
    [MaxLength(100, ErrorMessage = "عبارت جستجو نباید بیش از 100 کاراکتر باشد")]
    public string? SearchTerm { get; init; }

    /// <summary>
    /// فیلد مرتب‌سازی - امکان مرتب‌سازی بر اساس فیلدهای مختلف (createdAt, orderNumber, amount, status)
    /// </summary>
    public string? SortBy { get; init; }

    /// <summary>
    /// جهت مرتب‌سازی - صعودی (asc) یا نزولی (desc) - پیش‌فرض: نزولی
    /// </summary>
    public string SortDirection { get; init; } = "desc";
}