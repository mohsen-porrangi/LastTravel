using Carter;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using BuildingBlocks.Contracts;
using WalletPayment.Application.Features.Transactions.TransferMoney;

namespace WalletPayment.API.Endpoints.Transactions;

/// <summary>
/// Wallet transfer endpoint - انتقال بین کیف پول‌ها
/// </summary>
public class WalletTransferEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/transactions/transfer", TransferMoneyAsync)
            .WithName("TransferMoney")
            .WithTags("Transactions")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Transfer money between wallets";
                operation.Description = "Transfer money from current user's wallet to another user's wallet";
                return operation;
            });
    }

    public record TransferMoneyRequest(
        Guid ToUserId,
        decimal Amount,
        string Description,
        string? Reference = null
    );

    [Authorize]
    private static async Task<IResult> TransferMoneyAsync(
        TransferMoneyRequest request,
        IMediator mediator,
        ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        var fromUserId = currentUserService.GetCurrentUserId();

        var command = new TransferMoneyCommand
        {
            FromUserId = fromUserId,
            ToUserId = request.ToUserId,
            Amount = request.Amount,
            Description = request.Description,
            Reference = request.Reference
        };

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccessful)
        {
            return Results.BadRequest(new { error = result.ErrorMessage });
        }

        return Results.Ok(new
        {
            success = true,
            fromTransactionId = result.FromTransactionId,
            toTransactionId = result.ToTransactionId,
            transferAmount = result.TransferAmount,
            transferFee = result.TransferFee,
            fromWalletNewBalance = result.FromWalletNewBalance,
            toWalletNewBalance = result.ToWalletNewBalance,
            transferReference = result.TransferReference,
            processedAt = result.ProcessedAt
        });
    }
}