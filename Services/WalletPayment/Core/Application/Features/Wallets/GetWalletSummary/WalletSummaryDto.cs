using WalletPayment.Domain.Enums;
namespace WalletPayment.Application.Features.Wallets.GetWalletSummary;
/// <summary>
/// Wallet summary DTO
/// </summary>
public record WalletSummaryDto
{
    public Guid WalletId { get; init; }
    public Guid UserId { get; init; }
    public bool IsActive { get; init; }
    public decimal TotalBalanceInIrr { get; init; }
    public IEnumerable<CurrencyBalanceDto> CurrencyBalances { get; init; } = [];
    public IEnumerable<RecentTransactionDto> RecentTransactions { get; init; } = [];
    public WalletStatisticsDto Statistics { get; init; } = null!;
    public IEnumerable<BankAccountSummaryDto> BankAccounts { get; init; } = [];
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

/// <summary>
/// Recent transaction DTO
/// </summary>
public record RecentTransactionDto
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
}

/// <summary>
/// Wallet statistics DTO
/// </summary>
public record WalletStatisticsDto
{
    public int TotalTransactions { get; init; }
    public int SuccessfulTransactions { get; init; }
    public decimal TotalDeposits { get; init; }
    public decimal TotalWithdrawals { get; init; }
    public int CurrentMonthTransactions { get; init; }
}

/// <summary>
/// Bank account summary DTO
/// </summary>
public record BankAccountSummaryDto
{
    public Guid Id { get; init; }
    public string BankName { get; init; } = string.Empty;
    public string MaskedAccountNumber { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
    public bool IsVerified { get; init; }
}