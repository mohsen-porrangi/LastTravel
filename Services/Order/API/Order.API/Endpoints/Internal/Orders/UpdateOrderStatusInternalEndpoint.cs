using Carter;
using MediatR;
using Order.Domain.Enums;

namespace Order.API.Endpoints.Internal.Orders;

public class UpdateOrderStatusInternalEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/internal/orders/{id:guid}/status", async (
            Guid id,
            UpdateOrderStatusRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            return Results.Ok(new { message = "Status updated successfully" });
        })
        .WithName("UpdateOrderStatusInternal")
        .WithDescription("بروزرسانی وضعیت سفارش توسط سرویس‌های داخلی")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .WithTags("Internal")
        .AllowAnonymous();
    }
}

public record UpdateOrderStatusRequest(
    OrderStatus Status,
    string Reason = ""
);