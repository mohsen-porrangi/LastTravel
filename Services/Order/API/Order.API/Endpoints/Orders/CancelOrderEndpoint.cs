using Carter;
using MediatR;
using Order.Application.Orders.Commands.CancelOrder;

namespace Order.API.Endpoints.Orders;

public class CancelOrderEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders/{id:guid}/cancel", async (
            Guid id,
            CancelOrderRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new CancelOrderCommand(id, request.Reason);
            var result = await sender.Send(command, ct);

            return Results.Ok(new
            {
                success = true,
                message = "سفارش با موفقیت لغو شد",
                data = new
                {
                    orderId = result.OrderId,
                    orderNumber = result.OrderNumber,
                    cancelledAt = result.CancelledAt,
                    reason = result.Reason
                }
            });
        })
        .WithName("CancelOrder")
        .WithSummary("لغو سفارش")
        .WithDescription("لغو سفارش با رعایت قوانین کسب‌وکار")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .WithTags("Orders")
        .RequireAuthorization();
    }
}

public record CancelOrderRequest(string Reason);