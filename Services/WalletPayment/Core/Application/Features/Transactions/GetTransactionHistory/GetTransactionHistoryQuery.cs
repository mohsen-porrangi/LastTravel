using BuildingBlocks.CQRS;
using WalletPayment.Domain.Enums;

namespace WalletPayment.Application.Features.Transactions.GetTransactionHistory;

/// <summary>
/// Get transaction history query
/// </summary>
public record GetTransactionHistoryQuery : IQuery<GetTransactionHistoryResult>
{
    public Guid UserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public TransactionType? Type { get; init; }
    public TransactionDirection? Direction { get; init; }
    public CurrencyCode? Currency { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

/// <summary>
/// Transaction history result
/// </summary>
public record GetTransactionHistoryResult
{
    public IEnumerable<TransactionDto> Transactions { get; init; } = [];
    public PaginationInfo Pagination { get; init; } = null!;
}

/// <summary>
/// Transaction DTO
/// </summary>
public record TransactionDto
{
    public Guid Id { get; init; }
    public string TransactionNumber { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public CurrencyCode Currency { get; init; }
    public TransactionDirection Direction { get; init; }
    public TransactionType Type { get; init; }
    public TransactionStatus Status { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime TransactionDate { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public string? PaymentReferenceId { get; init; }
    public string? OrderContext { get; init; }
}

/// <summary>
/// Pagination information
/// </summary>
public record PaginationInfo
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public bool HasNextPage { get; init; }
    public bool HasPreviousPage { get; init; }
}
