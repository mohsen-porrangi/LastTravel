using Carter;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using BuildingBlocks.Contracts;
using WalletPayment.Application.Features.BankAccounts.GetBankAccounts;

namespace WalletPayment.API.Endpoints.BankAccounts;

/// <summary>
/// Get bank accounts endpoint
/// </summary>
public class GetBankAccountsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/bank-accounts", GetBankAccountsAsync)
            .WithName("GetBankAccounts")
            .WithTags("Bank Accounts")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get user bank accounts";
                operation.Description = "Get all bank accounts for the current user";
                return operation;
            });
    }

    [Authorize]
    private static async Task<IResult> GetBankAccountsAsync(
        IMediator mediator,
        ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        var query = new GetBankAccountsQuery
        {
            UserId = userId
        };

        var result = await mediator.Send(query, cancellationToken);

        return Results.Ok(new
        {
            success = true,
            bankAccounts = result.BankAccounts.Select(ba => new
            {
                id = ba.Id,
                bankName = ba.BankName,
                maskedAccountNumber = ba.MaskedAccountNumber,
                maskedCardNumber = ba.MaskedCardNumber,
                shabaNumber = ba.ShabaNumber,
                accountHolderName = ba.AccountHolderName,
                isVerified = ba.IsVerified,
                isDefault = ba.IsDefault,
                isActive = ba.IsActive,
                createdAt = ba.CreatedAt
            })
        });
    }
}