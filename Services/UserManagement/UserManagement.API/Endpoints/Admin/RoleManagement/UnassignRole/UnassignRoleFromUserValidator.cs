namespace UserManagement.API.Endpoints.Admin.RoleManagement.UnassignRole;

public class UnassignRoleFromUserCommandValidator : AbstractValidator<UnassignRoleFromUserCommand>
{
    public UnassignRoleFromUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر الزامی است.");

        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("شناسه نقش نامعتبر است.");
    }
}

