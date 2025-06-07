using Carter;
using MediatR;
using Order.Application.Orders.Commands.CreateOrder;

namespace Order.API.Endpoints.Internal.Orders;

public class CreateOrderInternalEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/internal/orders", async (
            CreateOrderInternalRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new CreateOrderCommand
            {
                //  UserId = request.UserId,
                ServiceType = request.ServiceType,
                SourceCode = request.SourceCode,
                DestinationCode = request.DestinationCode,
                SourceName = request.SourceName,
                DestinationName = request.DestinationName,
                DepartureDate = request.DepartureDate,
                ReturnDate = request.ReturnDate,
                Passengers = request.Passengers,
                //   FlightNumber = request.FlightNumber,
                //  TrainNumber = request.TrainNumber,
                //    ProviderId = request.ProviderId
            };

            var result = await sender.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("CreateOrderInternal")
        .WithDescription("ایجاد سفارش توسط سرویس‌های داخلی")
        .Produces<CreateOrderResult>(StatusCodes.Status200OK)
        .WithTags("Internal")
        .AllowAnonymous();
    }
}

public record CreateOrderInternalRequest(
    Guid UserId,
    Order.Domain.Enums.ServiceType ServiceType,
    int SourceCode,
    int DestinationCode,
    string SourceName,
    string DestinationName,
    DateTime DepartureDate,
    DateTime? ReturnDate,
    List<CreateOrderPassengerInfo> Passengers,
    string? FlightNumber,
    string? TrainNumber,
    int? ProviderId);