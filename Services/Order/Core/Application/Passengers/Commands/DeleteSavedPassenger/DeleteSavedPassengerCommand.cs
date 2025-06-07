using BuildingBlocks.CQRS;

namespace Order.Application.Passengers.Commands.DeleteSavedPassenger;

public record DeleteSavedPassengerCommand(long PassengerId, Guid UserId) : ICommand;
