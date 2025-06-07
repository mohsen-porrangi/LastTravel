using BuildingBlocks.CQRS;
using FluentValidation;
using WalletPayment.Domain.Common.Contracts;
using WalletPayment.Domain.Exceptions;

namespace WalletPayment.Application.Features.BankAccounts.RemoveBankAccount;

/// <summary>
/// Remove bank account command
/// </summary>
public record RemoveBankAccountCommand : ICommand<RemoveBankAccountResult>
{
    public Guid UserId { get; init; }
    public Guid BankAccountId { get; init; }
}

/// <summary>
/// Remove bank account result
/// </summary>
public record RemoveBankAccountResult
{
    public bool IsSuccessful { get; init; }
    public string? ErrorMessage { get; init; }
    public Guid BankAccountId { get; init; }
    public DateTime? RemovedAt { get; init; }
}
