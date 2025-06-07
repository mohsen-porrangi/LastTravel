using BuildingBlocks.Contracts;
using BuildingBlocks.CQRS;
using Microsoft.Extensions.Logging;
using Order.Domain.Contracts;
using Order.Domain.Entities;

namespace Order.Application.Passengers.Commands.SavePassenger;

public class SavePassengerCommandHandler(
    ISavedPassengerRepository passengerRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    ILogger<SavePassengerCommandHandler> logger) : ICommandHandler<SavePassengerCommand, SavePassengerResponse>
{
    public async Task<SavePassengerResponse> Handle(SavePassengerCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();
        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException("کاربر احراز نشده است.");

        // Check if passenger already exists
        var existingPassenger = await passengerRepository.GetByNationalCodeAsync(
            userId, request.NationalCode, cancellationToken);

        if (existingPassenger != null)
        {
            // Update existing passenger
            existingPassenger.UpdateInformation(
                request.FirstNameEn,
                request.LastNameEn,
                request.FirstNameFa,
                request.LastNameFa,
                request.PassportNumber);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new SavePassengerResponse(existingPassenger.Id);
        }
        //else
        //{
        //    throw new BadRequestException("مسافر با این کد ملی قبلاً ثبت شده است");
        //}

        // Create new passenger
        var passenger = new SavedPassenger(
            userId,
            request.FirstNameEn,
            request.LastNameEn,
            request.FirstNameFa,
            request.LastNameFa,
            request.NationalCode,
            request.PassportNumber,
            request.BirthDate,
            request.Gender);

        await passengerRepository.AddAsync(passenger, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Saved passenger {NationalCode} for user {UserId}",
           request.NationalCode, userId);
        return new SavePassengerResponse(passenger.Id);
    }
}