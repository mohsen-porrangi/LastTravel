namespace WalletPayment.Domain.Enums;
/// <summary>
/// وضعیت اعتبار B2B (آینده)
/// </summary>
public enum CreditStatus
{
    /// <summary>
    /// فعال
    /// </summary>
    Active = 1,

    /// <summary>
    /// تسویه شده
    /// </summary>
    Settled = 2,

    /// <summary>
    /// سررسید گذشته
    /// </summary>
    Overdue = 3,

    /// <summary>
    /// معلق
    /// </summary>
    Suspended = 4
}