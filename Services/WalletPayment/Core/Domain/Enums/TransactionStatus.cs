namespace WalletPayment.Domain.Enums;
/// <summary>
/// وضعیت تراکنش
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    /// در انتظار پردازش
    /// </summary>
    Pending = 1,

    /// <summary>
    /// تکمیل شده و موفق
    /// </summary>
    Completed = 2,

    /// <summary>
    /// در حال پردازش
    /// </summary>
    Processing = 3,

    /// <summary>
    /// ناموفق
    /// </summary>
    Failed = 4,

    /// <summary>
    /// استرداد شده
    /// </summary>
    Refunded = 5,

    /// <summary>
    /// لغو شده
    /// </summary>
    Cancelled = 6
}