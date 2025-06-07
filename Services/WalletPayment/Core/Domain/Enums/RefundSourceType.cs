using System.ComponentModel;

namespace WalletPayment.Domain.Enums;

/// <summary>
/// Refund Source Type - Specifies the source of refund operation
/// </summary>
public enum RefundSourceType
{
    /// <summary>
    /// Refund from a wallet transaction
    /// </summary>
    [Description("تراکنش کیف پول")]
    Transaction = 1,

    /// <summary>
    /// Refund from a payment gateway transaction
    /// </summary>
    [Description("تراکنش درگاه پرداخت")]
    Payment = 2
}