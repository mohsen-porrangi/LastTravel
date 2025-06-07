namespace WalletPayment.Domain.Common;

/// <summary>
/// Centralized domain error codes and messages
/// </summary>
public static class DomainErrors
{
    public static class Wallet
    {
        public const string NotFound = "WALLET_NOT_FOUND";
        public const string Inactive = "WALLET_INACTIVE";
        public const string AlreadyExists = "WALLET_ALREADY_EXISTS";
        public const string InvalidUser = "WALLET_INVALID_USER";

        public static readonly Dictionary<string, string> Messages = new()
        {
            [NotFound] = "کیف پول یافت نشد",
            [Inactive] = "کیف پول غیرفعال است",
            [AlreadyExists] = "کیف پول برای این کاربر قبلاً ایجاد شده است",
            [InvalidUser] = "شناسه کاربر نامعتبر است"
        };
    }

    public static class Transaction
    {
        public const string InvalidAmount = "TRANSACTION_INVALID_AMOUNT";
        public const string InvalidCurrency = "TRANSACTION_INVALID_CURRENCY";
        public const string InvalidType = "TRANSACTION_INVALID_TYPE";
        public const string InvalidDirection = "TRANSACTION_INVALID_DIRECTION";
        public const string InsufficientBalance = "TRANSACTION_INSUFFICIENT_BALANCE";
        public const string AlreadyProcessed = "TRANSACTION_ALREADY_PROCESSED";
        public const string CannotBeRefunded = "TRANSACTION_CANNOT_BE_REFUNDED";
        public const string DuplicateReference = "TRANSACTION_DUPLICATE_REFERENCE";

        public static readonly Dictionary<string, string> Messages = new()
        {
            [InvalidAmount] = "مبلغ تراکنش نامعتبر است",
            [InvalidCurrency] = "ارز انتخابی پشتیبانی نمی‌شود",
            [InvalidType] = "نوع تراکنش نامعتبر است",
            [InvalidDirection] = "جهت تراکنش نامعتبر است",
            [InsufficientBalance] = "موجودی کافی نیست",
            [AlreadyProcessed] = "تراکنش قبلاً پردازش شده است",
            [CannotBeRefunded] = "این تراکنش قابل استرداد نیست",
            [DuplicateReference] = "شماره مرجع تکراری است"
        };
    }

    public static class CurrencyAccount
    {
        public const string NotFound = "ACCOUNT_NOT_FOUND";
        public const string Inactive = "ACCOUNT_INACTIVE";
        public const string InvalidCurrency = "ACCOUNT_INVALID_CURRENCY";
        public const string AlreadyExists = "ACCOUNT_ALREADY_EXISTS";

        public static readonly Dictionary<string, string> Messages = new()
        {
            [NotFound] = "حساب ارزی یافت نشد",
            [Inactive] = "حساب ارزی غیرفعال است",
            [InvalidCurrency] = "ارز حساب نامعتبر است",
            [AlreadyExists] = "حساب با این ارز قبلاً ایجاد شده است"
        };
    }

    public static class BankAccount
    {
        public const string NotFound = "BANK_ACCOUNT_NOT_FOUND";
        public const string InvalidCardNumber = "BANK_ACCOUNT_INVALID_CARD";
        public const string InvalidShabaNumber = "BANK_ACCOUNT_INVALID_SHABA";
        public const string InvalidAccountNumber = "BANK_ACCOUNT_INVALID_NUMBER";
        public const string AlreadyExists = "BANK_ACCOUNT_ALREADY_EXISTS";

        public static readonly Dictionary<string, string> Messages = new()
        {
            [NotFound] = "حساب بانکی یافت نشد",
            [InvalidCardNumber] = "شماره کارت نامعتبر است",
            [InvalidShabaNumber] = "شماره شبا نامعتبر است",
            [InvalidAccountNumber] = "شماره حساب نامعتبر است",
            [AlreadyExists] = "حساب بانکی قبلاً ثبت شده است"
        };
    }

    public static class Credit
    {
        public const string NotFound = "CREDIT_NOT_FOUND";
        public const string AlreadyExists = "CREDIT_ALREADY_EXISTS";
        public const string InsufficientCredit = "CREDIT_INSUFFICIENT";
        public const string Overdue = "CREDIT_OVERDUE";
        public const string AlreadySettled = "CREDIT_ALREADY_SETTLED";
        public const string InvalidDueDate = "CREDIT_INVALID_DUE_DATE";

        public static readonly Dictionary<string, string> Messages = new()
        {
            [NotFound] = "اعتبار یافت نشد",
            [AlreadyExists] = "اعتبار فعال قبلاً وجود دارد",
            [InsufficientCredit] = "اعتبار کافی نیست",
            [Overdue] = "اعتبار سررسید شده است",
            [AlreadySettled] = "اعتبار قبلاً تسویه شده است",
            [InvalidDueDate] = "تاریخ سررسید نامعتبر است"
        };
    }

    public static class Payment
    {
        public const string GatewayError = "PAYMENT_GATEWAY_ERROR";
        public const string InvalidAmount = "PAYMENT_INVALID_AMOUNT";
        public const string InvalidCallback = "PAYMENT_INVALID_CALLBACK";
        public const string PaymentFailed = "PAYMENT_FAILED";
        public const string PaymentCancelled = "PAYMENT_CANCELLED";

        public static readonly Dictionary<string, string> Messages = new()
        {
            [GatewayError] = "خطا در ارتباط با درگاه پرداخت",
            [InvalidAmount] = "مبلغ پرداخت نامعتبر است",
            [InvalidCallback] = "پاسخ درگاه پرداخت نامعتبر است",
            [PaymentFailed] = "پرداخت ناموفق بود",
            [PaymentCancelled] = "پرداخت لغو شد"
        };
    }

    /// <summary>
    /// Get error message by code
    /// </summary>
    public static string GetMessage(string errorCode)
    {
        var allMessages = new Dictionary<string, string>();

        foreach (var category in new[] {
            Wallet.Messages,
            Transaction.Messages,
            CurrencyAccount.Messages,
            BankAccount.Messages,
            Credit.Messages,
            Payment.Messages
        })
        {
            foreach (var kvp in category)
            {
                allMessages[kvp.Key] = kvp.Value;
            }
        }

        return allMessages.TryGetValue(errorCode, out var message)
            ? message
            : "خطای نامشخص";
    }
}