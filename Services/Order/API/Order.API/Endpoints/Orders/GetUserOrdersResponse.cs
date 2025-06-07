namespace Order.API.Models.Order;

/// <summary>
/// پاسخ کامل دریافت لیست سفارشات کاربر شامل داده‌ها و اطلاعات صفحه‌بندی
/// </summary>
public record GetUserOrdersResponse
{
    /// <summary>
    /// لیست سفارشات صفحه جاری با اطلاعات خلاصه هر سفارش
    /// </summary>
    public List<OrderSummaryResponse> Orders { get; init; } = new();

    /// <summary>
    /// اطلاعات کامل صفحه‌بندی شامل تعداد کل، تعداد صفحات و وضعیت navigation
    /// </summary>
    public PaginationInfo Pagination { get; init; } = new();

    /// <summary>
    /// متادیتای اضافی برای client جهت بهبود user experience
    /// </summary>
    public ResponseMetadata Metadata { get; init; } = new();
}

/// <summary>
/// خلاصه اطلاعات هر سفارش برای نمایش در لیست
/// </summary>
public record OrderSummaryResponse
{
    /// <summary>
    /// شناسه یکتای سفارش
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// شماره سفارش قابل خواندن توسط کاربر (مثل ORD-20241201-001)
    /// </summary>
    public string OrderNumber { get; init; } = string.Empty;

    /// <summary>
    /// نوع سرویس سفارش (enum value)
    /// </summary>
    public Domain.Enums.ServiceType ServiceType { get; init; }

    /// <summary>
    /// نام فارسی نوع سرویس برای نمایش در UI
    /// </summary>
    public string ServiceTypeName { get; init; } = string.Empty;

    /// <summary>
    /// مبلغ کل سفارش شامل تمام هزینه‌ها و مالیات
    /// </summary>
    public decimal TotalAmount { get; init; }

    /// <summary>
    /// وضعیت فعلی سفارش (enum value)
    /// </summary>
    public Domain.Enums.OrderStatus Status { get; init; }

    /// <summary>
    /// نام فارسی وضعیت سفارش برای نمایش در UI
    /// </summary>
    public string StatusName { get; init; } = string.Empty;

    /// <summary>
    /// تعداد کل مسافران در این سفارش
    /// </summary>
    public int PassengerCount { get; init; }

    /// <summary>
    /// آیا این سفارش شامل بلیط برگشت است
    /// </summary>
    public bool HasReturn { get; init; }

    /// <summary>
    /// تاریخ و زمان ایجاد سفارش
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// اطلاعات خلاصه مسیر سفر برای نمایش سریع
    /// </summary>
    public RouteInfo? Route { get; init; }
}

/// <summary>
/// اطلاعات کامل صفحه‌بندی برای navigation در client
/// </summary>
public record PaginationInfo
{
    /// <summary>
    /// شماره صفحه فعلی
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// تعداد آیتم‌های موجود در صفحه فعلی
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// تعداد کل آیتم‌ها در تمام صفحات
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// تعداد کل صفحات
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// آیا صفحه قبلی وجود دارد
    /// </summary>
    public bool HasPreviousPage { get; init; }

    /// <summary>
    /// آیا صفحه بعدی وجود دارد
    /// </summary>
    public bool HasNextPage { get; init; }

    /// <summary>
    /// شماره اولین آیتم در صفحه فعلی (برای نمایش "نمایش x تا y از z")
    /// </summary>
    public int FirstItemOnPage { get; init; }

    /// <summary>
    /// شماره آخرین آیتم در صفحه فعلی
    /// </summary>
    public int LastItemOnPage { get; init; }
}

/// <summary>
/// اطلاعات خلاصه مسیر برای نمایش در لیست سفارشات
/// </summary>
public record RouteInfo
{
    /// <summary>
    /// نام شهر یا فرودگاه مبدا
    /// </summary>
    public string SourceName { get; init; } = string.Empty;

    /// <summary>
    /// نام شهر یا فرودگاه مقصد
    /// </summary>
    public string DestinationName { get; init; } = string.Empty;

    /// <summary>
    /// تاریخ و زمان حرکت
    /// </summary>
    public DateTime DepartureTime { get; init; }

    /// <summary>
    /// تاریخ و زمان رسیدن (اختیاری)
    /// </summary>
    public DateTime? ArrivalTime { get; init; }
}

/// <summary>
/// متادیتای پاسخ برای اطلاعات اضافی
/// </summary>
public record ResponseMetadata
{
    /// <summary>
    /// زمان تولید پاسخ
    /// </summary>
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// نسخه API
    /// </summary>
    public string ApiVersion { get; init; } = "v1";

    /// <summary>
    /// آیا فیلترهایی اعمال شده است
    /// </summary>
    public bool HasFilters { get; init; }

    /// <summary>
    /// تعداد فیلترهای فعال
    /// </summary>
    public int ActiveFiltersCount { get; init; }
}