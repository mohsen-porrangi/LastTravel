using BuildingBlocks.Enums;
using Order.Domain.Enums;

namespace Order.Application.Common.DTOs;

public record PassengerDto(
    string FirstNameEn,
    string LastNameEn,
    string FirstNameFa,
    string LastNameFa,
    DateTime BirthDate,
    Gender Gender,
    bool IsIranian,
    string? NationalCode,
    string? PassportNumber,
    AgeGroup AgeGroup
);