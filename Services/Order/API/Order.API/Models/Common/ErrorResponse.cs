namespace Order.API.Models.Common;

/// <summary>
/// مدل استاندارد برای پاسخ خطاهای API
/// </summary>
public record ErrorResponse
{
    /// <summary>
    /// پیام اصلی خطا برای نمایش به کاربر
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// جزئیات اضافی خطا (اختیاری)
    /// </summary>
    public string? Details { get; init; }

    /// <summary>
    /// نوع خطا برای دسته‌بندی در client
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// کد خطای داخلی برای tracking و debugging
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// زمان وقوع خطا
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// مدل خاص برای خطاهای اعتبارسنجی
/// </summary>
public record ValidationErrorResponse : ErrorResponse
{
    /// <summary>
    /// لیست خطاهای اعتبارسنجی فیلدهای مختلف
    /// </summary>
    public List<ValidationError> Errors { get; init; } = new();
}

/// <summary>
/// جزئیات خطای اعتبارسنجی هر فیلد
/// </summary>
public record ValidationError
{
    /// <summary>
    /// نام فیلد دارای خطا
    /// </summary>
    public string Field { get; init; } = string.Empty;

    /// <summary>
    /// پیام خطای اعتبارسنجی
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// مقدار نادرستی که کاربر وارد کرده است
    /// </summary>
    public string? AttemptedValue { get; init; }
}