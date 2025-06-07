using Carter;
using MediatR;
using Order.Application.Trains.Queries.GetTrainTickets;

namespace Order.API.Endpoints.Tickets;

public class GetTrainTicketsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/{orderId:guid}/trains", async (
            Guid orderId,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetTrainTicketsQuery(orderId);
            var result = await sender.Send(query, ct);

            return Results.Ok(new
            {
                orderNumber = result.OrderNumber,
                totalTickets = result.TotalTickets,
                issuedTickets = result.IssuedTickets,
                allTicketsIssued = result.AllTicketsIssued,
                tickets = result.Tickets
            });
        })
        .WithName("GetTrainTickets")
        .WithSummary("دریافت بلیط‌های قطار سفارش")
        .WithDescription("دریافت لیست بلیط‌های صادر شده قطار برای یک سفارش خاص")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .WithTags("Tickets")
        .RequireAuthorization();
    }
}