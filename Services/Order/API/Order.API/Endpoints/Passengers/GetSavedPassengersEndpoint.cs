using Carter;
using MediatR;
using Order.API.Extensions;
using Order.Application.Passengers.Queries.GetSavedPassengers;

namespace Order.API.Endpoints.Passengers;

public class GetSavedPassengersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/passengers", async (
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetSavedPassengersQuery();
            var result = await sender.Send(query, ct);

            return Results.Ok(result.ToSavedPassengersResponse());
        })
        .WithName("GetSavedPassengers")
        .WithSummary("دریافت لیست مسافران ذخیره شده")
        .WithDescription("دریافت لیست مسافران ذخیره شده کاربر")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithTags("Passengers")
        .RequireAuthorization();
    }
}