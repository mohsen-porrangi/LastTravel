using FluentValidation;
using Order.API.Models.Order;

namespace Order.API.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.ServiceType)
            .IsInEnum();

        RuleFor(x => x.SourceCode)
            .GreaterThan(0);

        RuleFor(x => x.DestinationCode)
            .GreaterThan(0)
            .NotEqual(x => x.SourceCode)
            .WithMessage("مبدا و مقصد نمی‌توانند یکسان باشند");

        RuleFor(x => x.SourceName)
            .NotEmpty();

        RuleFor(x => x.DestinationName)
            .NotEmpty();

        RuleFor(x => x.DepartureDate)
            .GreaterThan(DateTime.Now)
            .WithMessage("تاریخ حرکت باید در آینده باشد");

        When(x => x.ReturnDate.HasValue, () =>
        {
            RuleFor(x => x.ReturnDate!.Value)
                .GreaterThan(x => x.DepartureDate)
                .WithMessage("تاریخ برگشت باید بعد از تاریخ رفت باشد");
        });

        RuleFor(x => x.Passengers)
            .NotEmpty()
            .WithMessage("حداقل یک مسافر باید وارد شود");

        RuleForEach(x => x.Passengers)
            .SetValidator(new PassengerInfoValidator());
    }
}

public class PassengerInfoValidator : AbstractValidator<PassengerInfo>
{
    public PassengerInfoValidator()
    {
        RuleFor(x => x.FirstNameEn)
            .NotEmpty()
            .Matches(@"^[a-zA-Z\s]+$")
            .WithMessage("نام انگلیسی فقط باید شامل حروف انگلیسی باشد");

        RuleFor(x => x.LastNameEn)
            .NotEmpty()
            .Matches(@"^[a-zA-Z\s]+$")
            .WithMessage("نام خانوادگی انگلیسی فقط باید شامل حروف انگلیسی باشد");

        RuleFor(x => x.FirstNameFa)
            .NotEmpty()
            .Matches(@"^[\u0600-\u06FF\s]+$")
            .WithMessage("نام فارسی فقط باید شامل حروف فارسی باشد");

        RuleFor(x => x.LastNameFa)
            .NotEmpty()
            .Matches(@"^[\u0600-\u06FF\s]+$")
            .WithMessage("نام خانوادگی فارسی فقط باید شامل حروف فارسی باشد");

        RuleFor(x => x.BirthDate)
            .LessThan(DateTime.Today)
            .WithMessage("تاریخ تولد نامعتبر است");

        When(x => x.IsIranian, () =>
        {
            RuleFor(x => x.NationalCode)
                .NotEmpty()
                .Matches(@"^\d{10}$")
                .WithMessage("کد ملی باید 10 رقم باشد");
        });

        When(x => !x.IsIranian, () =>
        {
            RuleFor(x => x.PassportNumber)
                .NotEmpty()
                .WithMessage("شماره پاسپورت برای اتباع خارجی الزامی است");
        });
    }
}