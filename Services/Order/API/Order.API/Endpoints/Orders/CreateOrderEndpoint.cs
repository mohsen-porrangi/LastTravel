using BuildingBlocks.Contracts;
using Carter;
using MediatR;
using Order.API.Models.Order;
using Order.Application.Orders.Commands.CreateOrder;

namespace Order.API.Endpoints.Orders;

public class CreateOrderEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders", async (
            CreateOrderRequest request,
            ISender sender,
            ICurrentUserService currentUser,
            CancellationToken ct) =>
        {
            var command = new CreateOrderCommand
            {
                ServiceType = request.ServiceType,
                SourceCode = request.SourceCode,
                DestinationCode = request.DestinationCode,
                SourceName = request.SourceName,
                DestinationName = request.DestinationName,
                DepartureDate = request.DepartureDate,
                ReturnDate = request.ReturnDate,
                Passengers = request.Passengers.Select(p => new CreateOrderPassengerInfo
                (
                    FirstNameEn: p.FirstNameEn,
                    LastNameEn: p.LastNameEn,
                    FirstNameFa: p.FirstNameFa,
                    LastNameFa: p.LastNameFa,
                    BirthDate: p.BirthDate,
                    Gender: p.Gender,
                    IsIranian: p.IsIranian,
                    NationalCode: p.NationalCode,
                    PassportNumber: p.PassportNumber
                )).ToList(),
            };

            var result = await sender.Send(command, ct);

            return Results.Created($"/api/orders/{result.OrderId}", new
            {
                orderId = result.OrderId,
                orderNumber = result.OrderNumber
            });
        })
        .WithName("CreateOrder")
        .WithDescription("ایجاد سفارش جدید")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithTags("Orders")
        .RequireAuthorization();
    }
}