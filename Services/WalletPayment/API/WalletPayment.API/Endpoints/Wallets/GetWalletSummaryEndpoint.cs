using Carter;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using BuildingBlocks.Contracts;
using WalletPayment.Application.Features.Wallets.GetWalletSummary;

namespace WalletPayment.API.Endpoints.Wallets;

/// <summary>
/// Get wallet summary endpoint
/// </summary>
public class GetWalletSummaryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/wallets/summary", GetWalletSummaryAsync)
            .WithName("GetWalletSummary")
            .WithTags("Wallets")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get wallet summary";
                operation.Description = "Get comprehensive wallet summary including balance, transactions, and statistics";
                return operation;
            });
    }

    [Authorize]
    private static async Task<IResult> GetWalletSummaryAsync(
        IMediator mediator,
        ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        var query = new GetWalletSummaryQuery
        {
            UserId = userId
        };

        var result = await mediator.Send(query, cancellationToken);

        return Results.Ok(new
        {
            walletId = result.WalletId,
            userId = result.UserId,
            isActive = result.IsActive,
            totalBalanceInIrr = result.TotalBalanceInIrr,
            currencyBalances = result.CurrencyBalances.Select(cb => new
            {
                currency = cb.Currency.ToString(),
                balance = cb.Balance,
                isActive = cb.IsActive
            }),
            recentTransactions = result.RecentTransactions.Select(t => new
            {
                id = t.Id,
                transactionNumber = t.TransactionNumber,
                amount = t.Amount,
                currency = t.Currency.ToString(),
                direction = t.Direction.ToString(),
                type = t.Type.ToString(),
                status = t.Status.ToString(),
                description = t.Description,
                transactionDate = t.TransactionDate
            }),
            statistics = new
            {
                totalTransactions = result.Statistics.TotalTransactions,
                successfulTransactions = result.Statistics.SuccessfulTransactions,
                totalDeposits = result.Statistics.TotalDeposits,
                totalWithdrawals = result.Statistics.TotalWithdrawals,
                currentMonthTransactions = result.Statistics.CurrentMonthTransactions
            },
            bankAccounts = result.BankAccounts.Select(ba => new
            {
                id = ba.Id,
                bankName = ba.BankName,
                maskedAccountNumber = ba.MaskedAccountNumber,
                isDefault = ba.IsDefault,
                isVerified = ba.IsVerified
            })
        });
    }
}