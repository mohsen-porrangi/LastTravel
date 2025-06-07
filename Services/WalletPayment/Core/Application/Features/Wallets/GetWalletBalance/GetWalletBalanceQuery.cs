using BuildingBlocks.CQRS;
using WalletPayment.Domain.Enums;

namespace WalletPayment.Application.Features.Wallets.GetWalletBalance;

/// <summary>
/// Get wallet balance query
/// </summary>
public record GetWalletBalanceQuery : IQuery<WalletBalanceDto>
{
    public Guid UserId { get; init; }
}

/// <summary>
/// Wallet balance DTO
/// </summary>
public record WalletBalanceDto
{
    public Guid WalletId { get; init; }
    public Guid UserId { get; init; }
    public bool IsActive { get; init; }
    public decimal TotalBalanceInIrr { get; init; }
    public IEnumerable<CurrencyBalanceDto> CurrencyBalances { get; init; } = [];
}

/// <summary>
/// Currency balance DTO
/// </summary>
public record CurrencyBalanceDto
{
    public CurrencyCode Currency { get; init; }
    public decimal Balance { get; init; }
    public bool IsActive { get; init; }
}
