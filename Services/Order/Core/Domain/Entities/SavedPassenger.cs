using BuildingBlocks.Domain;
using BuildingBlocks.Enums;

namespace Order.Domain.Entities;

public class SavedPassenger : BaseEntity<long>
{
    public Guid UserId { get; private set; }
    public string FirstNameEn { get; private set; } = string.Empty;
    public string LastNameEn { get; private set; } = string.Empty;
    public string FirstNameFa { get; private set; } = string.Empty;
    public string LastNameFa { get; private set; } = string.Empty;
    public string NationalCode { get; private set; } = string.Empty;
    public string? PassportNumber { get; private set; }
    public DateTime BirthDate { get; private set; }
    public Gender Gender { get; private set; }
    public bool IsActive { get; private set; } = true;

    protected SavedPassenger() { }

    public SavedPassenger(
        Guid userId,
        string firstNameEn, string lastNameEn,
        string firstNameFa, string lastNameFa,
        string nationalCode, string? passportNumber,
        DateTime birthDate, Gender gender)
    {
        UserId = userId;
        FirstNameEn = firstNameEn;
        LastNameEn = lastNameEn;
        FirstNameFa = firstNameFa;
        LastNameFa = lastNameFa;
        NationalCode = nationalCode;
        PassportNumber = passportNumber;
        BirthDate = birthDate;
        Gender = gender;
        CreatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateInformation(
        string firstNameEn, string lastNameEn,
        string firstNameFa, string lastNameFa,
        string? passportNumber)
    {
        FirstNameEn = firstNameEn;
        LastNameEn = lastNameEn;
        FirstNameFa = firstNameFa;
        LastNameFa = lastNameFa;
        PassportNumber = passportNumber;
        UpdatedAt = DateTime.UtcNow;
    }
}