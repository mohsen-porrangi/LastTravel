using WalletPayment.Domain.DomainServices;
using WalletPayment.Domain.Enums;
using WalletPayment.Domain.ValueObjects;

namespace WalletPayment.Domain.DomainServices;

/// <summary>
/// Fee calculation service implementation
/// </summary>
public class FeeCalculationService : IFeeCalculationService
{
    /// <summary>
    /// Calculate fees based on transaction type
    /// </summary>
    public Money CalculateTransactionFee(TransactionType type, Money amount)
    {
        return type switch
        {
            TransactionType.Transfer => CalculateTransferFee(amount),
            TransactionType.Withdrawal => CalculateWithdrawalFee(amount),
            TransactionType.CreditSettlement => CalculateCreditFee(amount),
            _ => Money.Zero(amount.Currency) // No fee for other types
        };
    }

    /// <summary>
    /// Calculate currency conversion fee
    /// </summary>
    public Money CalculateConversionFee(Money fromAmount, CurrencyCode toCurrency)
    {
        if (fromAmount.Currency == toCurrency)
            return Money.Zero(fromAmount.Currency);

        // 0.25% conversion fee
        const decimal conversionRate = 0.0025m;
        var feeAmount = fromAmount.Value * conversionRate;

        return Money.Create(feeAmount, fromAmount.Currency);
    }

    /// <summary>
    /// Calculate early withdrawal fee
    /// </summary>
    public Money CalculateEarlyWithdrawalFee(Money amount, int daysEarly)
    {
        if (daysEarly <= 0)
            return Money.Zero(amount.Currency);

        // 0.1% per day early, max 5%
        var dailyRate = 0.001m;
        var maxRate = 0.05m;

        var feeRate = Math.Min(dailyRate * daysEarly, maxRate);
        var feeAmount = amount.Value * feeRate;

        return Money.Create(feeAmount, amount.Currency);
    }

    #region Private Methods

    /// <summary>
    /// Calculate transfer fee: 0.5% with min/max limits
    /// </summary>
    private Money CalculateTransferFee(Money amount)
    {
        const decimal feeRate = 0.005m; // 0.5%
        const decimal minFee = 1000m;   // 1000 IRR
        const decimal maxFee = 50000m;  // 50000 IRR

        var calculatedFee = amount.Value * feeRate;
        var actualFee = Math.Max(minFee, Math.Min(maxFee, calculatedFee));

        return Money.Create(actualFee, amount.Currency);
    }

    /// <summary>
    /// Calculate withdrawal fee
    /// </summary>
    private Money CalculateWithdrawalFee(Money amount)
    {
        // Flat fee for withdrawals
        const decimal withdrawalFee = 5000m; // 5000 IRR

        return Money.Create(withdrawalFee, CurrencyCode.IRR);
    }

    /// <summary>
    /// Calculate credit settlement fee
    /// </summary>
    private Money CalculateCreditFee(Money amount)
    {
        // 1% fee for credit settlements
        const decimal creditFeeRate = 0.01m;
        var feeAmount = amount.Value * creditFeeRate;

        return Money.Create(feeAmount, amount.Currency);
    }

    #endregion
}