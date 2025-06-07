using WalletPayment.Domain.Enums;

namespace WalletPayment.Domain.ValueObjects;

/// <summary>
/// Value Object برای شماره یکتای تراکنش
/// فرمت: TXN-YYYYMMDD-HHMMSS-XXXX
/// مثال: TXN-20250106-143025-0001
/// </summary>
public record TransactionNumber
{
    public string Value { get; }

    private TransactionNumber(string value)
    {
        Value = value;
    }

    /// <summary>
    /// تولید شماره تراکنش جدید
    /// </summary>
    public static TransactionNumber Generate()
    {
        var now = DateTime.UtcNow;
        var dateStr = now.ToString("yyyyMMdd");
        var timeStr = now.ToString("HHmmss");
        var randomPart = Random.Shared.Next(1000, 9999);

        return new TransactionNumber($"TXN-{dateStr}-{timeStr}-{randomPart}");
    }

    /// <summary>
    /// ایجاد از رشته موجود
    /// </summary>
    public static TransactionNumber FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("شماره تراکنش نمی‌تواند خالی باشد", nameof(value));

        if (!IsValidFormat(value))
            throw new ArgumentException($"فرمت شماره تراکنش نامعتبر: {value}", nameof(value));

        return new TransactionNumber(value);
    }

    /// <summary>
    /// بررسی فرمت معتبر شماره تراکنش
    /// </summary>
    private static bool IsValidFormat(string value)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(value, @"^TXN-\d{8}-\d{6}-\d{4}$");
    }

    public override string ToString() => Value;
}



