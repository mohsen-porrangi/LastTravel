using BuildingBlocks.CQRS;
using FluentValidation;
using WalletPayment.Application.Common.Interfaces;
using WalletPayment.Domain.Common.Contracts;
using WalletPayment.Domain.Common;
using WalletPayment.Domain.ValueObjects;
using WalletPayment.Domain.Enums;
using WalletPayment.Domain.Exceptions;

namespace WalletPayment.Application.Features.Transactions.IntegratedPurchase;

/// <summary>
/// Integrated purchase command - خرید یکپارچه با شارژ خودکار
/// </summary>
public record IntegratedPurchaseCommand : ICommand<IntegratedPurchaseResult>
{
    public Guid UserId { get; init; }
    public decimal TotalAmount { get; init; }
    public CurrencyCode Currency { get; init; } = CurrencyCode.IRR;
    public string OrderId { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string CallbackUrl { get; init; } = string.Empty;
    public bool UseCredit { get; init; } = false; // B2B future
}

/// <summary>
/// Integrated purchase result
/// </summary>
public record IntegratedPurchaseResult
{
    public bool IsSuccessful { get; init; }
    public PurchaseType PurchaseType { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal WalletBalance { get; init; }
    public decimal RequiredPayment { get; init; }
    public Guid? PurchaseTransactionId { get; init; }
    public Guid? PaymentTransactionId { get; init; }
    public string? PaymentUrl { get; init; }
    public string? Authority { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime? ProcessedAt { get; init; }
}

/// <summary>
/// Purchase type enumeration
/// </summary>
public enum PurchaseType
{
    FullWallet = 1,      // پرداخت کامل از کیف پول
    FullPayment = 2,     // پرداخت کامل از درگاه
    Mixed = 3,           // ترکیبی (کیف پول + درگاه)
    Credit = 4           // اعتباری (B2B)
}