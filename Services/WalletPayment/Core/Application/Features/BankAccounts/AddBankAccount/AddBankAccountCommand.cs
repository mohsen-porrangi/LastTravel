using BuildingBlocks.CQRS;
using FluentValidation;

namespace WalletPayment.Application.Features.BankAccounts.AddBankAccount;

/// <summary>
/// Add bank account command
/// </summary>
public record AddBankAccountCommand : ICommand<AddBankAccountResult>
{
    public Guid UserId { get; init; }
    public string BankName { get; init; } = string.Empty;
    public string AccountNumber { get; init; } = string.Empty;
    public string? CardNumber { get; init; }
    public string? ShabaNumber { get; init; }
    public string? AccountHolderName { get; init; }
}

/// <summary>
/// Add bank account result
/// </summary>
public record AddBankAccountResult
{
    public Guid BankAccountId { get; init; }
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