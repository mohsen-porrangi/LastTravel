using Carter;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using WalletPayment.Application.Features.Transactions.DirectDeposit;
using BuildingBlocks.Contracts;

namespace WalletPayment.API.Endpoints.Transactions;

/// <summary>
/// Direct deposit endpoint
/// </summary>
public class DirectDepositEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/transactions/direct-deposit", DirectDepositAsync)
            .WithName("DirectDeposit")
            .WithTags("Transactions")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Direct wallet deposit";
                operation.Description = "Create direct deposit to wallet via payment gateway";
                return operation;
            });
    }

    public record DirectDepositRequest(
        decimal Amount,
        string Description,
        string CallbackUrl
    );

    [Authorize]
    private static async Task<IResult> DirectDepositAsync(
        DirectDepositRequest request,
        IMediator mediator,
        ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        var command = new DirectDepositCommand
        {
            UserId = userId,
            Amount = request.Amount,
            Description = request.Description,
            CallbackUrl = request.CallbackUrl
        };

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccessful)
        {
            return Results.BadRequest(new { error = result.ErrorMessage });
        }

        return Results.Ok(new
        {
            isSuccessful = result.IsSuccessful,
            paymentUrl = result.PaymentUrl,
            authority = result.Authority,
            pendingTransactionId = result.PendingTransactionId
        });
    }
}
