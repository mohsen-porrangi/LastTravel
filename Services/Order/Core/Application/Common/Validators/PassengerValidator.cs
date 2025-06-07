using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using FluentValidation;
using System.Linq.Expressions;

namespace Order.Application.Common.Validators;
/// <summary>
/// ولیداتور قابل استفاده مجدد برای موجودیت‌های دارای مشخصات مسافر.
/// قوانین با استفاده از delegate تعریف می‌شن.
/// </summary>
public class PassengerValidator<T> : AbstractValidator<T>
{
    public PassengerValidator(
        Expression<Func<T, string>> firstNameEn,
        Expression<Func<T, string>> lastNameEn,
        Expression<Func<T, string>> firstNameFa,
        Expression<Func<T, string>> lastNameFa,
        Expression<Func<T, string>> nationalCode,
        Expression<Func<T, string?>> passportNumber,
        Expression<Func<T, DateTime>> birthDate,
        Expression<Func<T, Gender>> gender,
        Func<T, bool> isIranian)
    {
        RuleFor(firstNameEn)
            .NotEmpty().WithMessage("نام انگلیسی الزامی است")
            .Matches(@"^[a-zA-Z\s]+$").WithMessage("نام انگلیسی فقط باید شامل حروف انگلیسی باشد");

        RuleFor(lastNameEn)
            .NotEmpty().WithMessage("نام خانوادگی انگلیسی الزامی است")
            .Matches(@"^[a-zA-Z\s]+$").WithMessage("نام خانوادگی انگلیسی فقط باید شامل حروف انگلیسی باشد");

        RuleFor(firstNameFa)
            .NotEmpty().WithMessage("نام فارسی الزامی است")
            .Matches(@"^[\u0600-\u06FF\s]+$").WithMessage("نام فارسی فقط باید شامل حروف فارسی باشد");

        RuleFor(lastNameFa)
            .NotEmpty().WithMessage("نام خانوادگی فارسی الزامی است")
            .Matches(@"^[\u0600-\u06FF\s]+$").WithMessage("نام خانوادگی فارسی فقط باید شامل حروف فارسی باشد");

        RuleFor(birthDate)
            .NotEmpty().WithMessage("تاریخ تولد الزامی است")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("تاریخ تولد نمی‌تواند در آینده باشد");
        RuleFor(gender)
            .IsInEnum().WithMessage("جنسیت معتبر نیست");

        When(x => isIranian(x), () =>
        {
            RuleFor(nationalCode)
                .NotEmpty().WithMessage("کد ملی برای اتباع ایرانی الزامی است")
                .ValidationIranianNationalCode()
                .WithMessage("کد ملی نامعتبر است");
        });

        When(x => !isIranian(x), () =>
        {
            RuleFor(passportNumber)
                .NotEmpty().WithMessage("شماره پاسپورت برای اتباع خارجی الزامی است")
                .MinimumLength(6).WithMessage("شماره پاسپورت باید حداقل 6 کاراکتر باشد");
        });
    }
}