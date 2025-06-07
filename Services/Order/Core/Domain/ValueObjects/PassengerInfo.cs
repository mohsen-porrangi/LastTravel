using BuildingBlocks.Enums;
using Order.Domain.Enums;

namespace Order.Domain.ValueObjects;

public record PassengerInfo
{
    public string FirstNameEn { get; }
    public string LastNameEn { get; }
    public string FirstNameFa { get; }
    public string LastNameFa { get; }
    public DateTime BirthDate { get; }
    public Gender Gender { get; }
    public bool IsIranian { get; }
    public string? NationalCode { get; }
    public string? PassportNumber { get; }

    public PassengerInfo(
        string firstNameEn, string lastNameEn,
        string firstNameFa, string lastNameFa,
        DateTime birthDate, Gender gender,
        bool isIranian, string? nationalCode, string? passportNumber)
    {
        if (string.IsNullOrWhiteSpace(firstNameEn))
            throw new ArgumentException("English first name is required", nameof(firstNameEn));

        if (string.IsNullOrWhiteSpace(lastNameEn))
            throw new ArgumentException("English last name is required", nameof(lastNameEn));

        if (string.IsNullOrWhiteSpace(firstNameFa))
            throw new ArgumentException("Persian first name is required", nameof(firstNameFa));

        if (string.IsNullOrWhiteSpace(lastNameFa))
            throw new ArgumentException("Persian last name is required", nameof(lastNameFa));

        if (isIranian && string.IsNullOrWhiteSpace(nationalCode))
            throw new ArgumentException("National code is required for Iranian passengers", nameof(nationalCode));

        if (!isIranian && string.IsNullOrWhiteSpace(passportNumber))
            throw new ArgumentException("Passport number is required for non-Iranian passengers", nameof(passportNumber));

        FirstNameEn = firstNameEn;
        LastNameEn = lastNameEn;
        FirstNameFa = firstNameFa;
        LastNameFa = lastNameFa;
        BirthDate = birthDate;
        Gender = gender;
        IsIranian = isIranian;
        NationalCode = nationalCode;
        PassportNumber = passportNumber;
    }

    public string FullNameEn => $"{FirstNameEn} {LastNameEn}";
    public string FullNameFa => $"{FirstNameFa} {LastNameFa}";

    public int Age => CalculateAge();

    private int CalculateAge()
    {
        var today = DateTime.Today;
        var age = today.Year - BirthDate.Year;
        if (BirthDate.Date > today.AddYears(-age)) age--;
        return age;
    }

    public AgeGroup GetAgeGroup() => Age switch
    {
        < 2 => AgeGroup.Infant,
        < 12 => AgeGroup.Child,
        _ => AgeGroup.Adult
    };
}