using Carter;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using WalletPayment.Application.Features.Wallets.CreateWallet;
using BuildingBlocks.Contracts;

namespace WalletPayment.API.Endpoints.Wallets;

/// <summary>
/// Create wallet endpoint
/// </summary>
public class CreateWalletEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/wallets", CreateWalletAsync)
            .WithName("CreateWallet")
            .WithTags("Wallets")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Create new wallet";
                operation.Description = "Creates a new wallet for the current user with default IRR account";
                return operation;
            });
    }

    [Authorize]
    private static async Task<IResult> CreateWalletAsync(
        IMediator mediator,
        ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        var command = new CreateWalletCommand
        {
            UserId = userId,
            CreateDefaultAccount = true
        };

        var result = await mediator.Send(command, cancellationToken);

        return Results.Created($"/api/wallets/{result.WalletId}", new
        {
            walletId = result.WalletId,
            userId = result.UserId,
            defaultAccountId = result.DefaultAccountId,
            defaultCurrency = result.DefaultCurrency.ToString(),
            createdAt = result.CreatedAt
        });
    }
}