using Carter;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using BuildingBlocks.Contracts;
using WalletPayment.Application.Features.Transactions.RefundTransaction;

namespace WalletPayment.API.Endpoints.Transactions;

/// <summary>
/// Refund endpoint
/// </summary>
public class RefundEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // Create refund
        app.MapPost("/api/transactions/{transactionId:guid}/refund", CreateRefundAsync)
            .WithName("CreateRefund")
            .WithTags("Transactions")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Create transaction refund";
                operation.Description = "Create a refund for a completed transaction";
                return operation;
            });

        // Get refundable transactions
        app.MapGet("/api/transactions/refundable", GetRefundableTransactionsAsync)
            .WithName("GetRefundableTransactions")
            .WithTags("Transactions")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get refundable transactions";
                operation.Description = "Get list of transactions that can be refunded";
                return operation;
            });
    }

    public record CreateRefundRequest(
        string Reason,
        decimal? PartialAmount = null
    );

    [Authorize]
    private static async Task<IResult> CreateRefundAsync(
        Guid transactionId,
        CreateRefundRequest request,
        IMediator mediator,
        ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        var command = new RefundTransactionCommand
        {
            UserId = userId,
            OriginalTransactionId = transactionId,
            Reason = request.Reason,
            PartialAmount = request.PartialAmount
        };

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccessful)
        {
            return Results.BadRequest(new { error = result.ErrorMessage });
        }

        return Results.Ok(new
        {
            success = true,
            refundTransactionId = result.RefundTransactionId,
            originalTransactionId = result.OriginalTransactionId,
            refundAmount = result.RefundAmount,
            newWalletBalance = result.NewWalletBalance,
            processedAt = result.ProcessedAt
        });
    }

    [Authorize]
    private static async Task<IResult> GetRefundableTransactionsAsync(
        ICurrentUserService currentUserService,
        WalletPayment.Domain.Common.Contracts.IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        var refundableTransactions = await unitOfWork.Transactions.GetRefundableTransactionsAsync(
            userId, cancellationToken);

        var transactionDtos = refundableTransactions.Select(t => new
        {
            id = t.Id,
            transactionNumber = t.TransactionNumber.Value,
            amount = t.Amount.Value,
            currency = t.Amount.Currency.ToString(),
            type = t.Type.ToString(),
            description = t.Description,
            transactionDate = t.TransactionDate,
            processedAt = t.ProcessedAt,
            orderContext = t.OrderContext
        });

        return Results.Ok(new
        {
            success = true,
            refundableTransactions = transactionDtos
        });
    }
}