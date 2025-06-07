//using Carter;
//using MediatR;
//using WalletPayment.Application.Features.Transactions.RefundTransaction;

//namespace WalletPayment.API.Endpoints.Internal;

///// <summary>
///// Internal refund endpoints for service-to-service communication
///// </summary>
//public class InternalRefundEndpoints : ICarterModule
//{
//    public void AddRoutes(IEndpointRouteBuilder app)
//    {
//        var internalGroup = app.MapGroup("/api/internal/refunds")
//            .WithTags("Internal-Refunds");

//        // Process refund from Order service
//        internalGroup.MapPost("/process", ProcessRefundInternalAsync)
//            .WithName("ProcessRefundInternal")
//            .WithOpenApi(operation =>
//            {
//                operation.Summary = "Process refund (Internal)";
//                operation.Description = "Internal endpoint for processing refunds from Order service";
//                return operation;
//            });

//        // Check refund eligibility
//        internalGroup.MapGet("/check-eligibility/{transactionId:guid}", CheckRefundEligibilityAsync)
//            .WithName("CheckRefundEligibility")
//            .WithOpenApi(operation =>
//            {
//                operation.Summary = "Check refund eligibility";
//                operation.Description = "Check if a transaction is eligible for refund";
//                return operation;
//            });

//        // Get refund status
//        internalGroup.MapGet("/status/{refundTransactionId:guid}", GetRefundStatusAsync)
//            .WithName("GetRefundStatus")
//            .WithOpenApi(operation =>
//            {
//                operation.Summary = "Get refund status";
//                operation.Description = "Get the status of a refund transaction";
//                return operation;
//            });
//    }

//    public record ProcessRefundInternalRequest(
//        Guid UserId,
//        Guid OriginalTransactionId,
//        string Reason,
//        decimal? PartialAmount = null,
//        string? OrderNumber = null
//    );

//    private static async Task<IResult> ProcessRefundInternalAsync(
//        ProcessRefundInternalRequest request,
//        IMediator mediator,
//        CancellationToken cancellationToken)
//    {
//        try
//        {
//            var command = new RefundTransactionCommand
//            {
//                UserId = request.UserId,
//                OriginalTransactionId = request.OriginalTransactionId,
//                Reason = $"{request.Reason} - Order: {request.OrderNumber}",
//                PartialAmount = request.PartialAmount
//            };

//            var result = await mediator.Send(command, cancellationToken);

//            if (!result.IsSuccessful)
//            {
//                return Results.BadRequest(new
//                {
//                    success = false,
//                    error = result.ErrorMessage,
//                    originalTransactionId = request.OriginalTransactionId
//                });
//            }

//            return Results.Ok(new
//            {
//                success = true,
//                refundTransactionId = result.RefundTransactionId,
//                originalTransactionId = result.OriginalTransactionId,
//                refundAmount = result.RefundAmount,
//                newWalletBalance = result.NewWalletBalance,
//                processedAt = result.ProcessedAt
//            });
//        }
//        catch (Exception ex)
//        {
//            return Results.Problem(
//                detail: ex.Message,
//                statusCode: 500,
//                title: "Internal Refund Processing Error");
//        }
//    }

//    private static async Task<IResult> CheckRefundEligibilityAsync(
//        Guid transactionId,
//        WalletPayment.Domain.Common.Contracts.IUnitOfWork unitOfWork,
//        CancellationToken cancellationToken)
//    {
//        try
//        {
//            var transaction = await unitOfWork.Transactions.GetByIdAsync(transactionId, cancellationToken: cancellationToken);

//            if (transaction == null)
//            {
//                return Results.NotFound(new
//                {
//                    success = false,
//                    error = "تراکنش یافت نشد",
//                    transactionId
//                });
//            }

//            var isRefundable = transaction.IsRefundable();

//            return Results.Ok(new
//            {
//                success = true,
//                transactionId = transaction.Id,
//                isRefundable,
//                transactionNumber = transaction.TransactionNumber.Value,
//                amount = transaction.Amount.Value,
//                currency = transaction.Amount.Currency.ToString(),
//                transactionDate = transaction.TransactionDate,
//                status = transaction.Status.ToString(),
//                reason = isRefundable ? null : "تراکنش قابل استرداد نیست"
//            });
//        }
//        catch (Exception ex)
//        {
//            return Results.Problem(
//                detail: ex.Message,
//                statusCode: 500,
//                title: "Refund Eligibility Check Error");
//        }
//    }

//    private static async Task<IResult> GetRefundStatusAsync(
//        Guid refundTransactionId,
//        WalletPayment.Domain.Common.Contracts.IUnitOfWork unitOfWork,
//        CancellationToken cancellationToken)
//    {
//        try
//        {
//            var refundTransaction = await unitOfWork.Transactions.GetByIdAsync(refundTransactionId, cancellationToken: cancellationToken);

//            if (refundTransaction == null)
//            {
//                return Results.NotFound(new
//                {
//                    success = false,
//                    error = "تراکنش استرداد یافت نشد",
//                    refundTransactionId
//                });
//            }

//            return Results.Ok(new
//            {
//                success = true,
//                refundTransactionId = refundTransaction.Id,
//                originalTransactionId = refundTransaction.RelatedTransactionId,
//                transactionNumber = refundTransaction.TransactionNumber.Value,
//                amount = refundTransaction.Amount.Value,
//                currency = refundTransaction.Amount.Currency.ToString(),
//                status = refundTransaction.Status.ToString(),
//                description = refundTransaction.Description,
//                transactionDate = refundTransaction.TransactionDate,
//                processedAt = refundTransaction.ProcessedAt
//            });
//        }
//        catch (Exception ex)
//        {
//            return Results.Problem(
//                detail: ex.Message,
//                statusCode: 500,
//                title: "Get Refund Status Error");
//        }
//    }
//}