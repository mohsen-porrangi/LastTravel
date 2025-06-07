using Carter;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using BuildingBlocks.Contracts;

namespace WalletPayment.API.Endpoints.Wallets;

/// <summary>
/// Get wallet balance endpoint
/// </summary>
public class GetWalletBalanceEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/wallets/balance", GetWalletBalanceAsync)
            .WithName("GetWalletBalance")
            .WithTags("Wallets")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get wallet balance";
                operation.Description = "Get current wallet balance for all currencies";
                return operation;
            });
    }

    [Authorize]
    private static async Task<IResult> GetWalletBalanceAsync(
        ICurrentUserService currentUserService,
        WalletPayment.Domain.Common.Contracts.IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        var wallet = await unitOfWork.Wallets.GetByUserIdWithIncludesAsync(
            userId,
            includeCurrencyAccounts: true,
            cancellationToken: cancellationToken);

        if (wallet == null)
        {
            return Results.NotFound(new { error = "کیف پول یافت نشد" });
        }

        var balances = wallet.CurrencyAccounts
            .Where(a => a.IsActive && !a.IsDeleted)
            .Select(a => new
            {
                currency = a.Currency.ToString(),
                balance = a.Balance.Value,
                isActive = a.IsActive
            })
            .ToList();

        var totalBalanceInIrr = await unitOfWork.Wallets.GetTotalBalanceInIrrAsync(wallet.Id, cancellationToken);

        return Results.Ok(new
        {
            walletId = wallet.Id,
            userId = wallet.UserId,
            isActive = wallet.IsActive,
            totalBalanceInIrr = totalBalanceInIrr,
            currencyBalances = balances
        });
    }
}