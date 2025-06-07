using BuildingBlocks.Contracts;
using Carter;
using MediatR;
using Order.Application.Passengers.Commands.DeleteSavedPassenger;

namespace Order.API.Endpoints.Passengers;

public class DeleteSavedPassengerEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/passengers/{id:long}", async (
            long id,
            ISender sender,
            ICurrentUserService currentUser,
            CancellationToken ct) =>
        {
            var command = new DeleteSavedPassengerCommand(id, currentUser.GetCurrentUserId());
            await sender.Send(command, ct);
            return Results.NoContent();
        })
        .WithName("DeleteSavedPassenger")
        .WithDescription("حذف مسافر ذخیره شده")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithTags("Passengers")
        .RequireAuthorization();
    }
}