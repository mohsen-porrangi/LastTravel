using WalletPayment.Domain.Enums;
using WalletPayment.Domain.ValueObjects;

namespace WalletPayment.Domain.DomainServices;

/// <summary>
/// Fee calculation service interface
/// </summary>
public interface IFeeCalculationService
{
    /// <summary>
    /// Calculate transaction fee based on type and amount
    /// </summary>
    Money CalculateTransactionFee(TransactionType type, Money amount);

    /// <summary>
    /// Calculate currency conversion fee
    /// </summary>
    Money CalculateConversionFee(Money fromAmount, CurrencyCode toCurrency);

    /// <summary>
    /// Calculate early withdrawal fee
    /// </summary>
    Money CalculateEarlyWithdrawalFee(Money amount, int daysEarly);
}