using Order.Application.Common.Validators;

namespace Order.Application.Passengers.Commands.SavePassenger;

public class SavePassengerCommandValidator : PassengerValidator<SavePassengerCommand>
{
    public SavePassengerCommandValidator()
         : base(
             x => x.FirstNameEn,
             x => x.LastNameEn,
             x => x.FirstNameFa,
             x => x.LastNameFa,
             x => x.NationalCode,
             x => x.PassportNumber,
             x => x.BirthDate,
             x => x.Gender,
             x => !string.IsNullOrWhiteSpace(x.NationalCode) // یا هر شرطی که معیاره ایرانی بودن باشه
         )
    {

    }
}