using FluentValidation;
using Order.API.Models.Passenger;

namespace Order.API.Validators;

public class PassengerRequestValidator : AbstractValidator<PassengerRequest>
{
    public PassengerRequestValidator()
    {
        RuleFor(x => x.FirstNameEn)
            .NotEmpty()
            .Matches(@"^[a-zA-Z\s]+$");

        RuleFor(x => x.LastNameEn)
            .NotEmpty()
            .Matches(@"^[a-zA-Z\s]+$");

        RuleFor(x => x.FirstNameFa)
            .NotEmpty()
            .Matches(@"^[\u0600-\u06FF\s]+$");

        RuleFor(x => x.LastNameFa)
            .NotEmpty()
            .Matches(@"^[\u0600-\u06FF\s]+$");

        RuleFor(x => x.NationalCode)
            .NotEmpty()
            .Matches(@"^\d{10}$");

        RuleFor(x => x.BirthDate)
            .LessThan(DateTime.Today);
    }
}