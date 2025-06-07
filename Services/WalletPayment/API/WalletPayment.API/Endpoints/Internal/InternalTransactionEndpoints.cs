using Carter;
using WalletPayment.Domain.Common.Contracts;

namespace WalletPayment.API.Endpoints.Internal;

/// <summary>
/// Internal transaction endpoints for Order service
/// </summary>
public class InternalTransactionEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var internalGroup = app.MapGroup("/api/internal/transactions")
            .WithTags("Internal-Transactions");

        internalGroup.MapGet("/{transactionId:guid}/status", GetTransactionStatusAsync)
            .WithName("GetTransactionStatusInternal")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get transaction status (Internal)";
                operation.Description = "Get transaction status for Order service";
                return operation;
            });

        internalGroup.MapGet("/{transactionId:guid}/details", GetTransactionDetailsAsync)
            .WithName("GetTransactionDetailsInternal")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get transaction details (Internal)";
                operation.Description = "Get complete transaction details for Order service";
                return operation;
            });
    }

    private static async Task<IResult> GetTransactionStatusAsync(
        Guid transactionId,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        try
        {
            var transaction = await unitOfWork.Transactions.GetByIdAsync(
                transactionId, cancellationToken: cancellationToken);

            if (transaction == null)
            {
                return Results.NotFound(new
                {
                    success = false,
                    error = "تراکنش یافت نشد",
                    transactionId
                });
            }

            return Results.Ok(new
            {
                success = true,
                transactionId = transaction.Id,
                status = transaction.Status.ToString(),
                amount = transaction.Amount.Value,
                currency = transaction.Amount.Currency.ToString(),
                transactionDate = transaction.TransactionDate,
                processedAt = transaction.ProcessedAt
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: 500,
                title: "Get Transaction Status Error");
        }
    }

    private static async Task<IResult> GetTransactionDetailsAsync(
        Guid transactionId,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        try
        {
            var transaction = await unitOfWork.Transactions.GetByIdAsync(
                transactionId, cancellationToken: cancellationToken);

            if (transaction == null)
            {
                return Results.NotFound(new
                {
                    success = false,
                    error = "تراکنش یافت نشد",
                    transactionId
                });
            }

            return Results.Ok(new
            {
                success = true,
                id = transaction.Id,
                transactionNumber = transaction.TransactionNumber.Value,
                amount = transaction.Amount.Value,
                currency = transaction.Amount.Currency.ToString(),
                direction = transaction.Direction.ToString(),
                type = transaction.Type.ToString(),
                status = transaction.Status.ToString(),
                description = transaction.Description,
                orderContext = transaction.OrderContext,
                paymentReferenceId = transaction.PaymentReferenceId,
                transactionDate = transaction.TransactionDate,
                processedAt = transaction.ProcessedAt,
                isRefundable = transaction.IsRefundable(),
                userId = transaction.UserId,
                walletId = transaction.WalletId
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: 500,
                title: "Get Transaction Details Error");
        }
    }
}