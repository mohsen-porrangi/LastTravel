using Carter;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using WalletPayment.Application.Features.Transactions.IntegratedPurchase;
using BuildingBlocks.Contracts;

namespace WalletPayment.API.Endpoints.Transactions;

/// <summary>
/// Integrated purchase endpoint
/// </summary>
public class IntegratedPurchaseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/transactions/integrated-purchase", IntegratedPurchaseAsync)
            .WithName("IntegratedPurchase")
            .WithTags("Transactions")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Integrated purchase";
                operation.Description = "Process purchase with automatic wallet top-up if needed";
                return operation;
            });
    }

    public record IntegratedPurchaseRequest(
        decimal TotalAmount,
        string OrderId,
        string Description,
        string CallbackUrl,
        bool UseCredit = false
    );

    [Authorize]
    private static async Task<IResult> IntegratedPurchaseAsync(
        IntegratedPurchaseRequest request,
        IMediator mediator,
        ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        var command = new IntegratedPurchaseCommand
        {
            UserId = userId,
            TotalAmount = request.TotalAmount,
            OrderId = request.OrderId,
            Description = request.Description,
            CallbackUrl = request.CallbackUrl,
            UseCredit = request.UseCredit
        };

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccessful)
        {
            return Results.BadRequest(new { error = result.ErrorMessage });
        }

        return Results.Ok(new
        {
            isSuccessful = result.IsSuccessful,
            purchaseType = result.PurchaseType.ToString(),
            totalAmount = result.TotalAmount,
            walletBalance = result.WalletBalance,
            requiredPayment = result.RequiredPayment,
            purchaseTransactionId = result.PurchaseTransactionId,
            paymentTransactionId = result.PaymentTransactionId,
            paymentUrl = result.PaymentUrl,
            authority = result.Authority,
            processedAt = result.ProcessedAt
        });
    }
}