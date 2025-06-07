using Carter;
using MediatR;
using Order.API.Extensions;
using Order.Application.Orders.Queries.GetOrderDetails;

namespace Order.API.Endpoints.Orders;

public class GetOrderDetailsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/{id:guid}/details", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetOrderDetailsQuery(id);
            var result = await sender.Send(query, ct);

            return Results.Ok(result.ToApiResponse());
        })
        .WithName("GetOrderDetails")
        .WithSummary("دریافت جزئیات کامل سفارش")
        .WithDescription("دریافت جزئیات کامل سفارش شامل بلیط‌ها، تاریخچه وضعیت و تراکنش‌ها")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithTags("Orders")
        .RequireAuthorization();
    }
}
