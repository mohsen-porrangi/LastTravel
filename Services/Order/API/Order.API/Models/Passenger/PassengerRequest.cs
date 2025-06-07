namespace Order.API.Models.Passenger;

public record PassengerRequest
{
    public string FirstNameEn { get; init; } = string.Empty;
    public string LastNameEn { get; init; } = string.Empty;
    public string FirstNameFa { get; init; } = string.Empty;
    public string LastNameFa { get; init; } = string.Empty;
    public string NationalCode { get; init; } = string.Empty;
    public string? PassportNumber { get; init; }
    public DateTime BirthDate { get; init; }
    public BuildingBlocks.Enums.Gender Gender { get; init; }
}