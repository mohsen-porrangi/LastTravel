using WalletPayment.Domain.Enums;

namespace WalletPayment.Domain.ValueObjects;

/// <summary>
/// Value Object برای نمایش مبلغ با ارز
/// </summary>
public record Money
{
    public decimal Value { get; }
    public CurrencyCode Currency { get; }

    public Money(decimal value, CurrencyCode currency)
    {
        if (value < 0)
            throw new ArgumentException("مبلغ نمی‌تواند منفی باشد", nameof(value));

        ValidateCurrencyPrecision(value, currency);

        Value = value;
        Currency = currency;
    }

    /// <summary>
    /// Factory method to create Money with validation
    /// </summary>
    public static Money Create(decimal value, CurrencyCode currency)
    {
        return new Money(value, currency);
    }

    /// <summary>
    /// ایجاد Money با مقدار صفر
    /// </summary>
    public static Money Zero(CurrencyCode currency) => new(0, currency);

    /// <summary>
    /// ایجاد Money با ریال
    /// </summary>
    public static Money FromIrr(decimal amount) => new(amount, CurrencyCode.IRR);

    /// <summary>
    /// ایجاد Money با دلار (آینده)
    /// </summary>
    public static Money FromUsd(decimal amount) => new(amount, CurrencyCode.USD);

    /// <summary>
    /// ایجاد Money با یورو (آینده)
    /// </summary>
    public static Money FromEur(decimal amount) => new(amount, CurrencyCode.EUR);

    /// <summary>
    /// جمع دو مبلغ با ارز یکسان
    /// </summary>
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"نمی‌توان {Currency} را با {other.Currency} جمع کرد");

        return new Money(Value + other.Value, Currency);
    }

    /// <summary>
    /// تفریق دو مبلغ با ارز یکسان
    /// </summary>
    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"نمی‌توان {other.Currency} را از {Currency} کم کرد");

        return new Money(Value - other.Value, Currency);
    }

    /// <summary>
    /// مقایسه بزرگتر بودن
    /// </summary>
    public bool IsGreaterThan(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"نمی‌توان {Currency} را با {other.Currency} مقایسه کرد");

        return Value > other.Value;
    }

    /// <summary>
    /// بررسی کفایت مبلغ
    /// </summary>
    public bool IsSufficientFor(Money requiredAmount)
    {
        return Currency == requiredAmount.Currency && Value >= requiredAmount.Value;
    }

    /// <summary>
    /// اعتبارسنجی دقت ارز
    /// </summary>
    private static void ValidateCurrencyPrecision(decimal value, CurrencyCode currency)
    {
        var allowedDecimalPlaces = currency switch
        {
            CurrencyCode.IRR => 0, // ریال فقط عدد صحیح
            _ => 2 // سایر ارزها حداکثر 2 رقم اعشار
        };

        if (Math.Round(value, allowedDecimalPlaces) != value)
            throw new ArgumentException($"ارز {currency} حداکثر {allowedDecimalPlaces} رقم اعشار مجاز است", nameof(value));
    }

    public override string ToString() => $"{Value:N} {Currency}";
}