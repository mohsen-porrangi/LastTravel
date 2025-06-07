using BuildingBlocks.Messaging.Events;
using WalletPayment.Domain.Enums;

namespace WalletPayment.Domain.Events;

/// <summary>
/// رویداد ایجاد کیف پول جدید
/// </summary>
public record WalletCreatedEvent : IntegrationEvent
{
    public WalletCreatedEvent(Guid walletId, Guid userId)
    {
        WalletId = walletId;
        UserId = userId;
        Source = "WalletPayment";
    }

    public Guid WalletId { get; }
    public Guid UserId { get; }
}

/// <summary>
/// رویداد ایجاد حساب ارزی جدید
/// </summary>
public record CurrencyAccountCreatedEvent : IntegrationEvent
{
    public CurrencyAccountCreatedEvent(Guid walletId, Guid accountId, CurrencyCode currency)
    {
        WalletId = walletId;
        AccountId = accountId;
        Currency = currency;
        Source = "WalletPayment";
    }

    public Guid WalletId { get; }
    public Guid AccountId { get; }
    public CurrencyCode Currency { get; }
}

/// <summary>
/// رویداد شارژ کیف پول
/// </summary>
public record WalletDepositedEvent : IntegrationEvent
{
    public WalletDepositedEvent(Guid walletId, Guid accountId, decimal amount, CurrencyCode currency, string referenceId)
    {
        WalletId = walletId;
        AccountId = accountId;
        Amount = amount;
        Currency = currency;
        ReferenceId = referenceId;
        Source = "WalletPayment";
    }

    public Guid WalletId { get; }
    public Guid AccountId { get; }
    public decimal Amount { get; }
    public CurrencyCode Currency { get; }
    public string ReferenceId { get; }
}

/// <summary>
/// رویداد برداشت از کیف پول
/// </summary>
public record WalletWithdrawnEvent : IntegrationEvent
{
    public WalletWithdrawnEvent(Guid walletId, Guid accountId, decimal amount, CurrencyCode currency, string? orderId = null)
    {
        WalletId = walletId;
        AccountId = accountId;
        Amount = amount;
        Currency = currency;
        OrderId = orderId;
        Source = "WalletPayment";
    }

    public Guid WalletId { get; }
    public Guid AccountId { get; }
    public decimal Amount { get; }
    public CurrencyCode Currency { get; }
    public string? OrderId { get; }
}

/// <summary>
/// رویداد افزودن حساب بانکی
/// </summary>
public record BankAccountAddedEvent : IntegrationEvent
{
    public BankAccountAddedEvent(Guid walletId, Guid bankAccountId, string bankName, string accountNumber)
    {
        WalletId = walletId;
        BankAccountId = bankAccountId;
        BankName = bankName;
        AccountNumber = accountNumber;
        Source = "WalletPayment";
    }

    public Guid WalletId { get; }
    public Guid BankAccountId { get; }
    public string BankName { get; }
    public string AccountNumber { get; }
}