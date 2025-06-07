using BuildingBlocks.Exceptions;

namespace UserManagement.API.Endpoints.Profile.EditCurrentUser;

public class EditCurrentUserCommandValidator : AbstractValidator<EditCurrentUserCommand>
{
    public EditCurrentUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه هویتی نامعتبر است.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("نام الزامی است.")
            .MaximumLength(50).WithMessage("نام نباید بیش از ۵۰ کاراکتر باشد.");

        RuleFor(x => x.Family)
            .NotEmpty().WithMessage("نام خانوادگی الزامی است.")
            .MaximumLength(50).WithMessage("نام خانوادگی نباید بیش از ۵۰ کاراکتر باشد.");

        RuleFor(x => x.NationalCode)
            .ValidationIranianNationalCode()
            .When(x => !string.IsNullOrWhiteSpace(x.NationalCode));

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("مقدار جنسیت نامعتبر است.");

        RuleFor(x => x.BirthDate)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("تاریخ تولد نمی‌تواند در آینده باشد.");

        // اعتبارسنجی فیلدهای تغییر رمز عبور
        When(x => !string.IsNullOrEmpty(x.CurrentPassword) || !string.IsNullOrEmpty(x.NewPassword), () =>
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("رمز عبور فعلی الزامی است.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("رمز عبور جدید الزامی است.")
                .MinimumLength(6).WithMessage("رمز عبور جدید باید حداقل ۶ کاراکتر باشد.")
                .MaximumLength(50).WithMessage("رمز عبور جدید نمی‌تواند بیشتر از ۵۰ کاراکتر باشد.")
                .NotEqual(x => x.CurrentPassword).WithMessage("رمز عبور جدید نباید با رمز فعلی یکسان باشد.");
        });
    }
}