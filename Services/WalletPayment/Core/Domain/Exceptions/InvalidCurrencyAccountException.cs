using BuildingBlocks.Exceptions;
using WalletPayment.Domain.Enums;

namespace WalletPayment.Domain.Exceptions;

/// <summary>
/// Invalid currency account exception
/// </summary>
public class InvalidCurrencyAccountException : BadRequestException
{
    public InvalidCurrencyAccountException(CurrencyCode currency)
        : base("Invalid currency account", $"Currency account for {currency} not found or invalid")
    {
        Currency = currency;
    }

    public CurrencyCode Currency { get; }
}