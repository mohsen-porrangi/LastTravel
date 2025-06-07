using BuildingBlocks.Contracts;
using Carter;
using MediatR;
using Order.API.Models.Passenger;
using Order.Application.Passengers.Commands.SavePassenger;

namespace Order.API.Endpoints.Passengers;

public class SavePassengerEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/passengers", async (
            PassengerRequest request,
            ISender sender,
            ICurrentUserService currentUser,
            CancellationToken ct) =>
        {
            var command = new SavePassengerCommand
            {
                UserId = currentUser.GetCurrentUserId(),
                FirstNameEn = request.FirstNameEn,
                LastNameEn = request.LastNameEn,
                FirstNameFa = request.FirstNameFa,
                LastNameFa = request.LastNameFa,
                NationalCode = request.NationalCode,
                PassportNumber = request.PassportNumber,
                BirthDate = request.BirthDate,
                Gender = request.Gender
            };

            var result = await sender.Send(command, ct);
            return Results.Created($"/api/passengers/{result.PassengerId}", new { id = result.PassengerId });
        })
        .WithName("SavePassenger")
        .WithDescription("ذخیره اطلاعات مسافر")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithTags("Passengers")
        .RequireAuthorization();
    }
}
