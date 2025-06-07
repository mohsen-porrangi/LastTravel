using BuildingBlocks.Contracts;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using MediatR;
using Order.Domain.Contracts;

namespace Order.Application.Passengers.Commands.DeleteSavedPassenger;

public class DeleteSavedPassengerCommandHandler(
    ISavedPassengerRepository passengerRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteSavedPassengerCommand>
{
    public async Task<Unit> Handle(DeleteSavedPassengerCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        var passenger = await passengerRepository.GetByIdAsync(request.PassengerId, cancellationToken: cancellationToken, track: true)
            ?? throw new NotFoundException("مسافر یافت نشد");

        if (passenger.UserId != userId)
            throw new ForbiddenDomainException("شما اجازه حذف این مسافر را ندارید");

        passenger.Deactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}