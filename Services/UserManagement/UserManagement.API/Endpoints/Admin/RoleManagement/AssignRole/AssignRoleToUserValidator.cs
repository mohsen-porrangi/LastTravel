namespace UserManagement.API.Endpoints.Admin.RoleManagement.AssignRole;

public class AssignRoleToUserCommandValidator : AbstractValidator<AssignRoleToUserCommand>
{
    public AssignRoleToUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر الزامی است.");

        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("شناسه نقش نامعتبر است.");
    }
}
