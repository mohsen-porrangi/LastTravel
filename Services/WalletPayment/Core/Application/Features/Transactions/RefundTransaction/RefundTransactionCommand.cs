using BuildingBlocks.CQRS;
using FluentValidation;

namespace WalletPayment.Application.Features.Transactions.RefundTransaction;

/// <summary>
/// Refund transaction command
/// </summary>
public record RefundTransactionCommand : ICommand<RefundTransactionResult>
{
    public Guid UserId { get; init; }
    public Guid OriginalTransactionId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public decimal? PartialAmount { get; init; } // For partial refunds
}

/// <summary>
/// Refund transaction result
/// </summary>
public record RefundTransactionResult
{
    public bool IsSuccessful { get; init; }
    public Guid? RefundTransactionId { get; init; }
    public Guid OriginalTransactionId { get; init; }
    public decimal RefundAmount { get; init; }
    public decimal NewWalletBalance { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime? ProcessedAt { get; init; }
}
