namespace WalletPayment.Domain.Enums;

/// <summary>
/// جهت تراکنش - ورودی یا خروجی
/// </summary>
public enum TransactionDirection
{
    /// <summary>
    /// واریز (افزایش موجودی)
    /// </summary>
    In = 1,

    /// <summary>
    /// برداشت (کاهش موجودی)
    /// </summary>
    Out = 2
}