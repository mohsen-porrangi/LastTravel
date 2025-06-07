namespace WalletPayment.Domain.Enums;

/// <summary>
/// نوع تراکنش در سیستم کیف پول
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// شارژ مستقیم کیف پول
    /// </summary>
    Deposit = 1,

    /// <summary>
    /// برداشت از کیف پول (خرید)
    /// </summary>
    Withdrawal = 2,

    /// <summary>
    /// خرید (استفاده از موجودی)
    /// </summary>
    Purchase = 3,

    /// <summary>
    /// استرداد وجه به کیف پول
    /// </summary>
    Refund = 4,

    /// <summary>
    /// انتقال بین کیف پول‌ها (آینده)
    /// </summary>
    Transfer = 5,

    /// <summary>
    /// کارمزد تراکنش
    /// </summary>
    Fee = 6,

    /// <summary>
    /// تسویه اعتبار B2B (آینده)
    /// </summary>
    CreditSettlement = 7
}