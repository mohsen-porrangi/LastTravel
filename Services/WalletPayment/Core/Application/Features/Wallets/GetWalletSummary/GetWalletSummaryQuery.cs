using BuildingBlocks.CQRS;
using WalletPayment.Domain.Common.Contracts;
using WalletPayment.Domain.Exceptions;
using WalletPayment.Domain.Enums;

namespace WalletPayment.Application.Features.Wallets.GetWalletSummary;

/// <summary>
/// Get wallet summary query
/// </summary>
public record GetWalletSummaryQuery : IQuery<WalletSummaryDto>
{
    public Guid UserId { get; init; }
}


