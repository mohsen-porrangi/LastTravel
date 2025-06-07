using Carter;
using MediatR;
using Order.Application.Flights.Queries.GetFlightTickets;

namespace Order.API.Endpoints.Tickets;

public class GetFlightTicketsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/{orderId:guid}/flights", async (
            Guid orderId,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetFlightTicketsQuery(orderId);
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
        .WithName("GetFlightTickets")
        .WithSummary("دریافت بلیط‌های پرواز سفارش")
        .WithDescription("دریافت لیست بلیط‌های صادر شده پرواز برای یک سفارش خاص")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .WithTags("Tickets")
        .RequireAuthorization();
    }
}