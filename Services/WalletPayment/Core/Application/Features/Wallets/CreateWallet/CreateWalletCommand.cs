using BuildingBlocks.CQRS;
using FluentValidation;
using WalletPayment.Domain.Common.Contracts;
using WalletPayment.Domain.Common;
using WalletPayment.Domain.Enums;
using WalletPayment.Domain.Exceptions;

namespace WalletPayment.Application.Features.Wallets.CreateWallet;

/// <summary>
/// Create wallet command
/// </summary>
public record CreateWalletCommand : ICommand<CreateWalletResult>
{
    public Guid UserId { get; init; }
    public bool CreateDefaultAccount { get; init; } = true;
}

/// <summary>
/// Create wallet result
/// </summary>
public record CreateWalletResult
{
    public Guid WalletId { get; init; }
    public Guid UserId { get; init; }
    public Guid? DefaultAccountId { get; init; }
    public CurrencyCode DefaultCurrency { get; init; } = CurrencyCode.IRR;
    public DateTime CreatedAt { get; init; }
}
