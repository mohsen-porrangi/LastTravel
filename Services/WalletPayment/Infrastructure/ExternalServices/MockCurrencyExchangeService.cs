using Microsoft.Extensions.Logging;
using WalletPayment.Application.Common.Interfaces;
using WalletPayment.Domain.ValueObjects;
using WalletPayment.Domain.Enums;

namespace WalletPayment.Infrastructure.Services;

/// <summary>
/// Currency exchange service implementation (Mock for now)
/// </summary>
public class CurrencyExchangeService : ICurrencyExchangeService
{
    private readonly ILogger<CurrencyExchangeService> _logger;
    private readonly Dictionary<(CurrencyCode From, CurrencyCode To), decimal> _mockRates;

    public CurrencyExchangeService(ILogger<CurrencyExchangeService> logger)
    {
        _logger = logger;

        // Mock exchange rates (will be replaced with real API)
        _mockRates = new Dictionary<(CurrencyCode, CurrencyCode), decimal>
        {
            { (CurrencyCode.USD, CurrencyCode.IRR), 42000m },
            { (CurrencyCode.EUR, CurrencyCode.IRR), 46000m },
            { (CurrencyCode.GBP, CurrencyCode.IRR), 53000m },
            { (CurrencyCode.AED, CurrencyCode.IRR), 11500m },
            { (CurrencyCode.IRR, CurrencyCode.USD), 1m / 42000m },
            { (CurrencyCode.IRR, CurrencyCode.EUR), 1m / 46000m },
            { (CurrencyCode.IRR, CurrencyCode.GBP), 1m / 53000m },
            { (CurrencyCode.IRR, CurrencyCode.AED), 1m / 11500m }
        };
    }

    public async Task<ExchangeRateInfo> GetExchangeRateAsync(
        CurrencyCode from,
        CurrencyCode to,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting exchange rate from {From} to {To}", from, to);

        if (from == to)
        {
            return ExchangeRateInfo.Create(from, to, 1.0m, "Same Currency");
        }

        await Task.Delay(100, cancellationToken); // Simulate API call

        if (_mockRates.TryGetValue((from, to), out var rate))
        {
            return ExchangeRateInfo.Create(from, to, rate, "Mock Exchange Service");
        }

        throw new NotSupportedException($"Exchange rate from {from} to {to} is not supported");
    }

    public async Task<Money> ConvertAsync(
        Money amount,
        CurrencyCode targetCurrency,
        CancellationToken cancellationToken = default)
    {
        if (amount.Currency == targetCurrency)
            return amount;

        var exchangeRate = await GetExchangeRateAsync(amount.Currency, targetCurrency, cancellationToken);
        return exchangeRate.Convert(amount);
    }

    public async Task<IEnumerable<ExchangeRateInfo>> GetAllRatesAsync(
        CurrencyCode baseCurrency = CurrencyCode.IRR,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken); // Simulate API call

        var rates = new List<ExchangeRateInfo>();
        var supportedCurrencies = new[] { CurrencyCode.USD, CurrencyCode.EUR, CurrencyCode.GBP, CurrencyCode.AED };

        foreach (var currency in supportedCurrencies)
        {
            if (currency == baseCurrency) continue;

            if (_mockRates.TryGetValue((baseCurrency, currency), out var rate))
            {
                rates.Add(ExchangeRateInfo.Create(
                    baseCurrency,
                    currency,
                    rate,
                    "Mock Exchange Service"));
            }
        }

        return rates;
    }

    public async Task<bool> IsSupportedAsync(
        CurrencyCode from,
        CurrencyCode to,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken); // Simulate API call
        return from == to || _mockRates.ContainsKey((from, to));
    }
}