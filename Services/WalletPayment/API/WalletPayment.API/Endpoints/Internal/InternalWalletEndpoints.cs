using Carter;
using MediatR;
using WalletPayment.Application.Features.Wallets.CreateWallet;

namespace WalletPayment.API.Endpoints.Internal;

/// <summary>
/// Internal wallet endpoints for service-to-service communication
/// </summary>
public class InternalWalletEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // Internal endpoints don't require user authentication
        var internalGroup = app.MapGroup("/api/internal/wallets")
            .WithTags("Internal-Wallets");

        internalGroup.MapPost("/create", CreateWalletInternalAsync)
            .WithName("CreateWalletInternal")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Create wallet (Internal)";
                operation.Description = "Internal endpoint for creating wallets from other services";
                return operation;
            });

        internalGroup.MapGet("/{userId:guid}/exists", CheckWalletExistsAsync)
            .WithName("CheckWalletExists")
            .WithOpenApi();
    }

    public record CreateWalletInternalRequest(Guid UserId);

    private static async Task<IResult> CreateWalletInternalAsync(
        CreateWalletInternalRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateWalletCommand
        {
            UserId = request.UserId,
            CreateDefaultAccount = true
        };

        var result = await mediator.Send(command, cancellationToken);

        return Results.Ok(new
        {
            success = true,
            walletId = result.WalletId,
            userId = result.UserId,
            defaultAccountId = result.DefaultAccountId,
            createdAt = result.CreatedAt
        });
    }

    private static async Task<IResult> CheckWalletExistsAsync(
        Guid userId,
        WalletPayment.Domain.Common.Contracts.IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var exists = await unitOfWork.Wallets.UserHasWalletAsync(userId, cancellationToken);

        return Results.Ok(new
        {
            userId = userId,
            hasWallet = exists
        });
    }
}