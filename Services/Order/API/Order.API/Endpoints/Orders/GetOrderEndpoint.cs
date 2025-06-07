using BuildingBlocks.Contracts;
using Carter;
using MediatR;
using Order.API.Models.Order;
using Order.Application.Orders.Queries.GetOrderById;

namespace Order.API.Endpoints.Orders;

public class GetOrderEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/{id:guid}", async (
            Guid id,
            ISender sender,
            ICurrentUserService currentUser,
            CancellationToken ct) =>
        {
            var query = new GetOrderByIdQuery(id, IncludeOrder.All);
            var result = await sender.Send(query, ct);

            var response = new OrderResponse
            {
                Id = result.Id,
                OrderNumber = result.OrderNumber,
                ServiceType = result.ServiceType,
                TotalAmount = result.TotalAmount,
                Status = result.Status,
                PassengerCount = result.PassengerCount,
                HasReturn = result.HasReturn,
                CreatedAt = result.CreatedAt
            };

            return Results.Ok(response);
        })
        .WithName("GetOrder")
        .WithDescription("دریافت اطلاعات سفارش")
        .Produces<OrderResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithTags("Orders")
        .RequireAuthorization();
    }
}