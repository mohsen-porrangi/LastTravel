using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;

namespace Order.Application.Passengers.Commands.SavePassenger;

public record SavePassengerCommand : ICommand<SavePassengerResponse>
{
    public Guid UserId { get; set; } //For Admin Call
    public string FirstNameEn { get; init; } = string.Empty;
    public string LastNameEn { get; init; } = string.Empty;
    public string FirstNameFa { get; init; } = string.Empty;
    public string LastNameFa { get; init; } = string.Empty;
    public string NationalCode { get; init; } = string.Empty;
    public string? PassportNumber { get; init; }
    public DateTime BirthDate { get; init; }
    public Gender Gender { get; init; }
}

public record SavePassengerResponse(long PassengerId);