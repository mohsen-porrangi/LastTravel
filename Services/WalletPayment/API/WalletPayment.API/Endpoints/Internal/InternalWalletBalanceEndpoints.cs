using Carter;
using WalletPayment.Domain.Common.Contracts;

namespace WalletPayment.API.Endpoints.Internal;

/// <summary>
/// Internal wallet balance endpoints for Order service
/// </summary>
public class InternalWalletBalanceEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var internalGroup = app.MapGroup("/api/internal/wallets")
            .WithTags("Internal-Wallets");

        internalGroup.MapGet("/{userId:guid}/balance", GetWalletBalanceAsync)
            .WithName("GetWalletBalanceInternal")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get wallet balance (Internal)";
                operation.Description = "Get user wallet balance for Order service";
                return operation;
            });

        internalGroup.MapGet("/{userId:guid}/status", GetWalletStatusAsync)
            .WithName("GetWalletStatusInternal")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get wallet status (Internal)";
                operation.Description = "Get comprehensive wallet status for Order service";
                return operation;
            });

        internalGroup.MapPost("/{userId:guid}/check-affordability", CheckAffordabilityAsync)
            .WithName("CheckAffordabilityInternal")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Check affordability (Internal)";
                operation.Description = "Check if user can afford specific amount";
                return operation;
            });
    }

    private static async Task<IResult> GetWalletBalanceAsync(
        Guid userId,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        try
        {
            var wallet = await unitOfWork.Wallets.GetByUserIdWithIncludesAsync(
                userId, includeCurrencyAccounts: true, cancellationToken: cancellationToken);

            if (wallet == null)
            {
                return Results.Ok(new
                {
                    success = false,
                    hasWallet = false,
                    totalBalanceInIrr = 0m,
                    currencyBalances = Array.Empty<object>()
                });
            }

            var totalBalance = await unitOfWork.Wallets.GetTotalBalanceInIrrAsync(wallet.Id, cancellationToken);

            var currencyBalances = wallet.CurrencyAccounts
                .Where(a => a.IsActive && !a.IsDeleted)
                .Select(a => new
                {
                    currency = a.Currency.ToString(),
                    balance = a.Balance.Value
                });

            return Results.Ok(new
            {
                success = true,
                hasWallet = true,
                totalBalanceInIrr = totalBalance,
                currencyBalances
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: 500,
                title: "Get Wallet Balance Error");
        }
    }

    private static async Task<IResult> GetWalletStatusAsync(
        Guid userId,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        try
        {
            var wallet = await unitOfWork.Wallets.GetByUserIdWithIncludesAsync(
                userId, includeCurrencyAccounts: true, cancellationToken: cancellationToken);

            if (wallet == null)
            {
                return Results.Ok(new
                {
                    success = true,
                    hasWallet = false,
                    isActive = false,
                    totalBalanceInIrr = 0m,
                    canMakePayment = false
                });
            }

            var totalBalance = await unitOfWork.Wallets.GetTotalBalanceInIrrAsync(wallet.Id, cancellationToken);

            return Results.Ok(new
            {
                success = true,
                hasWallet = true,
                isActive = wallet.IsActive,
                totalBalanceInIrr = totalBalance,
                canMakePayment = wallet.IsActive && totalBalance > 0
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: 500,
                title: "Get Wallet Status Error");
        }
    }

    public record CheckAffordabilityRequest(
        decimal Amount,
        string Currency = "IRR"
    );

    private static async Task<IResult> CheckAffordabilityAsync(
        Guid userId,
        CheckAffordabilityRequest request,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        try
        {
            var wallet = await unitOfWork.Wallets.GetByUserIdWithIncludesAsync(
                userId, includeCurrencyAccounts: true, cancellationToken: cancellationToken);

            if (wallet == null)
            {
                return Results.Ok(new
                {
                    success = true,
                    canAfford = false,
                    reason = "کیف پول یافت نشد",
                    availableBalance = 0m,
                    requiredAmount = request.Amount,
                    shortfall = request.Amount
                });
            }

            if (!wallet.IsActive)
            {
                return Results.Ok(new
                {
                    success = true,
                    canAfford = false,
                    reason = "کیف پول غیرفعال است",
                    availableBalance = 0m,
                    requiredAmount = request.Amount,
                    shortfall = request.Amount
                });
            }

            var currency = WalletPayment.Domain.Enums.CurrencyCode.IRR;
            var account = wallet.GetCurrencyAccount(currency);
            var availableBalance = account?.Balance.Value ?? 0m;

            var canAfford = availableBalance >= request.Amount;
            var shortfall = canAfford ? 0m : request.Amount - availableBalance;

            return Results.Ok(new
            {
                success = true,
                canAfford,
                reason = canAfford ? "موجودی کافی است" : "موجودی کافی نیست",
                availableBalance,
                requiredAmount = request.Amount,
                shortfall
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: 500,
                title: "Check Affordability Error");
        }
    }
}