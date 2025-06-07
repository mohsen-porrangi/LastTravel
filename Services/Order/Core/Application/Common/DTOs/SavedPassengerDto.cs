using BuildingBlocks.Enums;
using Order.Domain.Enums;

namespace Order.Application.Common.DTOs;

/// <summary>
/// مسافر ذخیره شده
/// </summary>
public record SavedPassengerDto
{
    public long Id { get; init; }
    public string FirstNameEn { get; init; } = string.Empty;
    public string LastNameEn { get; init; } = string.Empty;
    public string FirstNameFa { get; init; } = string.Empty;
    public string LastNameFa { get; init; } = string.Empty;
    public string FullNameFa { get; init; } = string.Empty;
    public string FullNameEn { get; init; } = string.Empty;
    public string NationalCode { get; init; } = string.Empty;
    public string? PassportNumber { get; init; }
    public DateTime BirthDate { get; init; }
    public Gender Gender { get; init; }
    public string GenderName { get; init; } = string.Empty;
    public AgeGroup AgeGroup { get; init; }
    public string AgeGroupName { get; init; } = string.Empty;
    public int Age { get; init; }
    public bool IsIranian { get; init; }
    public DateTime CreatedAt { get; init; }
}