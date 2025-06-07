using BuildingBlocks.Exceptions;
using FluentValidation;

namespace Order.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.ServiceType)
            .IsInEnum().WithMessage("نوع سرویس معتبر نیست");

        RuleFor(x => x.SourceCode)
            .GreaterThan(0).WithMessage("کد مبدا معتبر نیست");

        RuleFor(x => x.DestinationCode)
            .GreaterThan(0).WithMessage("کد مقصد معتبر نیست")
            .NotEqual(x => x.SourceCode).WithMessage("مبدا و مقصد نمی‌توانند یکسان باشند");

        RuleFor(x => x.SourceName)
            .NotEmpty().WithMessage("نام مبدا الزامی است")
            .MaximumLength(100).WithMessage("نام مبدا نباید بیش از 100 کاراکتر باشد");

        RuleFor(x => x.DestinationName)
            .NotEmpty().WithMessage("نام مقصد الزامی است")
            .MaximumLength(100).WithMessage("نام مقصد نباید بیش از 100 کاراکتر باشد");

        RuleFor(x => x.DepartureDate)
            .GreaterThan(DateTime.Today).WithMessage("تاریخ رفت باید در آینده باشد");

        RuleFor(x => x.ReturnDate)
            .GreaterThan(x => x.DepartureDate)
            .WithMessage("تاریخ برگشت باید بعد از تاریخ رفت باشد")
            .When(x => x.ReturnDate.HasValue);

        RuleFor(x => x.Passengers)
            .NotEmpty().WithMessage("حداقل یک مسافر الزامی است")
            .Must(x => x.Count <= 10).WithMessage("حداکثر 10 مسافر در هر سفارش مجاز است");

        //  Validation برای اطلاعات پرواز/قطار
        RuleFor(x => x.FlightNumber)
            .NotEmpty().WithMessage("شماره پرواز الزامی است")
            .When(x => x.ServiceType == Domain.Enums.ServiceType.DomesticFlight ||
                      x.ServiceType == Domain.Enums.ServiceType.InternationalFlight);

        RuleFor(x => x.TrainNumber)
            .NotEmpty().WithMessage("شماره قطار الزامی است")
            .When(x => x.ServiceType == Domain.Enums.ServiceType.Train);

        RuleFor(x => x.ProviderId)
            .GreaterThan(0).WithMessage("شرکت ارائه‌دهنده باید انتخاب شود");

        RuleFor(x => x.BasePrice)
            .GreaterThan(0).WithMessage("قیمت پایه باید بیشتر از صفر باشد");

        //  Validation ساده‌تر برای مسافران
        RuleForEach(x => x.Passengers).ChildRules(passenger =>
        {
            passenger.RuleFor(x => x.FirstNameEn)
                .NotEmpty().WithMessage("نام انگلیسی الزامی است")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("نام انگلیسی فقط باید شامل حروف انگلیسی باشد")
                .MaximumLength(50).WithMessage("نام انگلیسی نباید بیش از 50 کاراکتر باشد");

            passenger.RuleFor(x => x.LastNameEn)
                .NotEmpty().WithMessage("نام خانوادگی انگلیسی الزامی است")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("نام خانوادگی انگلیسی فقط باید شامل حروف انگلیسی باشد")
                .MaximumLength(50).WithMessage("نام خانوادگی انگلیسی نباید بیش از 50 کاراکتر باشد");

            passenger.RuleFor(x => x.FirstNameFa)
                .NotEmpty().WithMessage("نام فارسی الزامی است")
                .Matches(@"^[\u0600-\u06FF\s]+$").WithMessage("نام فارسی فقط باید شامل حروف فارسی باشد")
                .MaximumLength(50).WithMessage("نام فارسی نباید بیش از 50 کاراکتر باشد");

            passenger.RuleFor(x => x.LastNameFa)
                .NotEmpty().WithMessage("نام خانوادگی فارسی الزامی است")
                .Matches(@"^[\u0600-\u06FF\s]+$").WithMessage("نام خانوادگی فارسی فقط باید شامل حروف فارسی باشد")
                .MaximumLength(50).WithMessage("نام خانوادگی فارسی نباید بیش از 50 کاراکتر باشد");

            passenger.RuleFor(x => x.BirthDate)
                .ValidateBirthDate();

            passenger.RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("جنسیت معتبر نیست");

            //  Validation شرطی برای کد ملی و پاسپورت
            passenger.RuleFor(x => x.NationalCode!)
                .ValidationIranianNationalCode()
                .When(x => x.IsIranian);

            passenger.RuleFor(x => x.PassportNumber)
                .ValidatePassportNumber()
                .When(x => !x.IsIranian);

            // اطمینان از وجود یکی از کد ملی یا پاسپورت
            passenger.RuleFor(x => x)
                .Must(x => (x.IsIranian && !string.IsNullOrEmpty(x.NationalCode)) ||
                          (!x.IsIranian && !string.IsNullOrEmpty(x.PassportNumber)))
                .WithMessage("برای اتباع ایرانی کد ملی و برای اتباع خارجی شماره پاسپورت الزامی است");
        });
    }
}