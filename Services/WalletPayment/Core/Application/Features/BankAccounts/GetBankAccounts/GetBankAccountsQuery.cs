using BuildingBlocks.CQRS;

namespace WalletPayment.Application.Features.BankAccounts.GetBankAccounts;

/// <summary>
/// Get bank accounts query
/// </summary>
public record GetBankAccountsQuery : IQuery<GetBankAccountsResult>
{
    public Guid UserId { get; init; }
}

/// <summary>
/// Get bank accounts result
/// </summary>
public record GetBankAccountsResult
{
    public IEnumerable<BankAccountDto> BankAccounts { get; init; } = [];
}

/// <summary>
/// Bank account DTO
/// </summary>
public record BankAccountDto
{
    public Guid Id { get; init; }
    public string BankName { get; init; } = string.Empty;
    public string MaskedAccountNumber { get; init; } = string.Empty;
    public string MaskedCardNumber { get; init; } = string.Empty;
    public string? ShabaNumber { get; init; }
    public string? AccountHolderName { get; init; }
    public bool IsVerified { get; init; }
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
