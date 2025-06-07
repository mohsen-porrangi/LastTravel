using WalletPayment.Domain.ValueObjects;
using WalletPayment.Domain.Enums;

namespace WalletPayment.Domain.Common;

/// <summary>
/// Central business rules for wallet domain
/// ✅ اصلاح شده: حذف duplicates، استفاده از DomainValidationRules
/// </summary>
public static class BusinessRules
{
    public static class Amounts
    {
        public static readonly Money MinimumTransactionAmount = Money.Create(1000, CurrencyCode.IRR); // 1000 ریال
        public static readonly Money MaximumSingleTransactionAmount = Money.Create(100_000_000, CurrencyCode.IRR); // 100 میلیون ریال
        public static readonly Money MaximumDailyTransactionAmount = Money.Create(500_000_000, CurrencyCode.IRR); // 500 میلیون ریال

        /// <summary>
        /// Validate transaction amount
        /// ✅ استفاده از DomainValidationRules
        /// </summary>
        public static bool IsValidTransactionAmount(Money amount)
        {
            return DomainValidationRules.Financial.IsValidTransactionAmount(amount);
        }

        /// <summary>
        /// Check if amount exceeds daily limit
        /// </summary>
        public static bool ExceedsDailyLimit(Money amount, Money dailyUsage)
        {
            if (amount.Currency != dailyUsage.Currency)
                return false;

            return (dailyUsage.Value + amount.Value) > MaximumDailyTransactionAmount.Value;
        }
    }

    public static class Currency
    {
        public static readonly CurrencyCode DefaultCurrency = CurrencyCode.IRR;
        public static readonly HashSet<CurrencyCode> SupportedCurrencies = new()
        {
            CurrencyCode.IRR
            // TODO: Add USD, EUR when exchange service is ready
        };

        /// <summary>
        /// Check if currency is supported
        /// </summary>
        public static bool IsSupportedCurrency(CurrencyCode currency)
        {
            return SupportedCurrencies.Contains(currency);
        }

        /// <summary>
        /// Validate currency precision
        /// ✅ استفاده از DomainValidationRules
        /// </summary>
        public static bool IsValidCurrencyPrecision(decimal amount, CurrencyCode currency)
        {
            return DomainValidationRules.Financial.IsValidCurrencyPrecision(amount, currency);
        }
    }

    public static class Wallet
    {
        public const int MaxCurrencyAccountsPerWallet = 5;
        public const int MaxBankAccountsPerWallet = 10;

        /// <summary>
        /// Check if wallet can have more currency accounts
        /// </summary>
        public static bool CanAddCurrencyAccount(int currentAccountCount)
        {
            return currentAccountCount < MaxCurrencyAccountsPerWallet;
        }

        /// <summary>
        /// Check if wallet can have more bank accounts
        /// </summary>
        public static bool CanAddBankAccount(int currentBankAccountCount)
        {
            return currentBankAccountCount < MaxBankAccountsPerWallet;
        }
    }

    public static class Transaction
    {
        public const int RefundAllowedDays = 30;
        public const int MaxTransactionsPerDay = 100;

        /// <summary>
        /// Check if transaction can be refunded
        /// </summary>
        public static bool CanBeRefunded(
            TransactionStatus status,
            TransactionDirection direction,
            TransactionType type,
            DateTime transactionDate)
        {
            // Must be completed
            if (status != TransactionStatus.Completed)
                return false;

            // Must be outgoing
            if (direction != TransactionDirection.Out)
                return false;

            // Cannot refund refunds
            if (type == TransactionType.Refund)
                return false;

            // Must be within allowed timeframe
            if (transactionDate < DateTime.UtcNow.AddDays(-RefundAllowedDays))
                return false;

            return true;
        }

        /// <summary>
        /// Validate transaction type for direction
        /// </summary>
        public static bool IsValidTypeForDirection(TransactionType type, TransactionDirection direction)
        {
            return direction switch
            {
                TransactionDirection.In => type is TransactionType.Deposit or TransactionType.Refund,
                TransactionDirection.Out => type is TransactionType.Withdrawal
                    or TransactionType.Purchase
                    or TransactionType.Transfer
                    or TransactionType.Fee,
                _ => false
            };
        }
    }

    public static class Credit
    {
        public static readonly Money MaximumCreditLimit = Money.Create(1_000_000_000, CurrencyCode.IRR); // 1 میلیارد ریال
        public const int MaximumCreditDurationDays = 90;
        public const int CreditWarningDaysBefore = 7;

        /// <summary>
        /// Check if credit amount is valid
        /// ✅ استفاده از DomainValidationRules
        /// </summary>
        public static bool IsValidCreditAmount(Money amount)
        {
            return DomainValidationRules.Financial.IsValidCreditAmount(amount);
        }

        /// <summary>
        /// Check if credit due date is valid
        /// ✅ استفاده از DomainValidationRules
        /// </summary>
        public static bool IsValidCreditDueDate(DateTime dueDate)
        {
            return DomainValidationRules.Business.IsValidCreditDueDate(dueDate);
        }

        /// <summary>
        /// Check if credit needs warning
        /// </summary>
        public static bool NeedsWarning(DateTime dueDate)
        {
            return dueDate <= DateTime.UtcNow.AddDays(CreditWarningDaysBefore);
        }
    }

    // ✅ BankAccount class حذف شد - همه validation در DomainValidationRules است

    public static class Payment
    {
        public const int PaymentTimeoutMinutes = 30;
        public const int MaxCallbackRetries = 3;

        /// <summary>
        /// Check if payment has timed out
        /// ✅ استفاده از DomainValidationRules
        /// </summary>
        public static bool HasPaymentTimedOut(DateTime paymentInitiatedAt)
        {
            return DomainValidationRules.Business.HasPaymentTimedOut(paymentInitiatedAt);
        }
    }
}