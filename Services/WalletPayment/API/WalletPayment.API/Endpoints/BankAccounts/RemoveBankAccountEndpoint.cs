using Carter;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using BuildingBlocks.Contracts;
using WalletPayment.Application.Features.BankAccounts.RemoveBankAccount;

namespace WalletPayment.API.Endpoints.BankAccounts;

/// <summary>
/// Remove bank account endpoint
/// </summary>
public class RemoveBankAccountEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/bank-accounts/{bankAccountId:guid}", RemoveBankAccountAsync)
            .WithName("RemoveBankAccount")
            .WithTags("Bank Accounts")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Remove bank account";
                operation.Description = "Remove (soft delete) a bank account from user's wallet";
                return operation;
            });
    }

    [Authorize]
    private static async Task<IResult> RemoveBankAccountAsync(
        Guid bankAccountId,
        IMediator mediator,
        ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        var command = new RemoveBankAccountCommand
        {
            UserId = userId,
            BankAccountId = bankAccountId
        };

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccessful)
        {
            return Results.BadRequest(new { error = result.ErrorMessage });
        }

        return Results.Ok(new
        {
            success = true,
            message = "حساب بانکی با موفقیت حذف شد",
            bankAccountId = result.BankAccountId,
            removedAt = result.RemovedAt
        });
    }
}