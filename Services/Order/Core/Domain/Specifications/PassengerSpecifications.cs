using Order.Domain.Entities;
using System.Linq.Expressions;

namespace Order.Domain.Specifications;

public static class PassengerSpecifications
{
    public static Expression<Func<SavedPassenger, bool>> ByUser(Guid userId)
        => passenger => passenger.UserId == userId;

    public static Expression<Func<SavedPassenger, bool>> Active()
        => passenger => passenger.IsActive;

    public static Expression<Func<SavedPassenger, bool>> ByNationalCode(string nationalCode)
        => passenger => passenger.NationalCode == nationalCode;

    public static Expression<Func<SavedPassenger, bool>> Adults()
        => passenger => DateTime.Today.Year - passenger.BirthDate.Year >= 12;

    public static Expression<Func<SavedPassenger, bool>> HasValidPassport()
        => passenger => !string.IsNullOrWhiteSpace(passenger.PassportNumber);
}