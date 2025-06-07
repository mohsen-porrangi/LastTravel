using WalletPayment.Domain.ValueObjects;
using WalletPayment.Domain.Enums;

namespace WalletPayment.Application.Common.Interfaces;

/// <summary>
/// Currency exchange service interface (Future implementation)
/// </summary>
public interface ICurrencyExchangeService
{
    /// <summary>
    /// Get current exchange rate between currencies
    /// </summary>
    Task<ExchangeRateInfo> GetExchangeRateAsync(
        CurrencyCode from,
        CurrencyCode to,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Convert amount from one currency to another
    /// </summary>
    Task<Money> ConvertAsync(
        Money amount,
        CurrencyCode targetCurrency,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all supported exchange rates
    /// </summary>
    Task<IEnumerable<ExchangeRateInfo>> GetAllRatesAsync(
        CurrencyCode baseCurrency = CurrencyCode.IRR,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if currency pair is supported
    /// </summary>
    Task<bool> IsSupportedAsync(
        CurrencyCode from,
        CurrencyCode to,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Exchange rate information DTO
/// </summary>
public record ExchangeRateInfo
{
    public CurrencyCode SourceCurrency { get; init; }
    public CurrencyCode TargetCurrency { get; init; }
    public decimal Rate { get; init; }
    public DateTime LastUpdated { get; init; }
    public string Source { get; init; } = string.Empty;
    public DateTime ValidUntil { get; init; }

    /// <summary>
    /// Check if rate is still valid
    /// </summary>
    public bool IsValid => DateTime.UtcNow <= ValidUntil;

    /// <summary>
    /// Convert amount using this rate
    /// </summary>
    public Money Convert(Money amount)
    {
        if (amount.Currency != SourceCurrency)
            throw new ArgumentException($"Amount currency {amount.Currency} does not match rate source currency {SourceCurrency}");

        if (!IsValid)
            throw new InvalidOperationException("Exchange rate has expired");

        return Money.Create(amount.Value * Rate, TargetCurrency);
    }

    /// <summary>
    /// Create exchange rate info
    /// </summary>
    public static ExchangeRateInfo Create(
        CurrencyCode from,
        CurrencyCode to,
        decimal rate,
        string source = "Mock",
        int validForMinutes = 15)
    {
        var now = DateTime.UtcNow;
        return new ExchangeRateInfo
        {
            SourceCurrency = from,
            TargetCurrency = to,
            Rate = rate,
            LastUpdated = now,
            ValidUntil = now.AddMinutes(validForMinutes),
            Source = source
        };
    }
}
