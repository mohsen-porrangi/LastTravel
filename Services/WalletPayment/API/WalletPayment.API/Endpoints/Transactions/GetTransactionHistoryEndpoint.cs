using Carter;
using Microsoft.AspNetCore.Authorization;
using BuildingBlocks.Contracts;
using WalletPayment.Domain.Enums;

namespace WalletPayment.API.Endpoints.Transactions;

/// <summary>
/// Get transaction history endpoint
/// </summary>
public class GetTransactionHistoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/transactions/history", GetTransactionHistoryAsync)
            .WithName("GetTransactionHistory")
            .WithTags("Transactions")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get transaction history";
                operation.Description = "Get paginated transaction history for current user";
                return operation;
            });
    }

    [Authorize]
    private static async Task<IResult> GetTransactionHistoryAsync(
        ICurrentUserService currentUserService,
        WalletPayment.Domain.Common.Contracts.IUnitOfWork unitOfWork,
        int page = 1,
        int pageSize = 20,
        TransactionType? type = null,
        TransactionDirection? direction = null,
        CurrencyCode? currency = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUserService.GetCurrentUserId();

        var (transactions, totalCount) = await unitOfWork.Transactions.GetUserTransactionsAsync(
            userId, page, pageSize, type, direction, currency, fromDate, toDate, cancellationToken);

        var transactionDtos = transactions.Select(t => new
        {
            id = t.Id,
            transactionNumber = t.TransactionNumber.Value,
            amount = t.Amount.Value,
            currency = t.Amount.Currency.ToString(),
            direction = t.Direction.ToString(),
            type = t.Type.ToString(),
            status = t.Status.ToString(),
            description = t.Description,
            transactionDate = t.TransactionDate,
            processedAt = t.ProcessedAt,
            paymentReferenceId = t.PaymentReferenceId,
            orderContext = t.OrderContext
        }).ToList();

        return Results.Ok(new
        {
            transactions = transactionDtos,
            pagination = new
            {
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                hasNextPage = page * pageSize < totalCount,
                hasPreviousPage = page > 1
            }
        });
    }
}