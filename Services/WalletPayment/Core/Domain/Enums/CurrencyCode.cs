namespace WalletPayment.Domain.Enums;

/// <summary>
/// کدهای ارزی پشتیبانی شده در سیستم
/// فعلاً فقط ریال، آینده سایر ارزها اضافه می‌شود
/// </summary>
public enum CurrencyCode
{
    /// <summary>
    /// ریال ایران - ارز پیش‌فرض
    /// </summary>
    IRR = 1,

    /// <summary>
    /// دلار آمریکا - آینده
    /// </summary>
    USD = 2,

    /// <summary>
    /// یورو - آینده
    /// </summary>
    EUR = 3,

    /// <summary>
    /// پوند انگلیس - آینده
    /// </summary>
    GBP = 4,

    /// <summary>
    /// درهم امارات - آینده
    /// </summary>
    AED = 5
}