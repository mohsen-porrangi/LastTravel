using BuildingBlocks.Exceptions;

namespace UserManagement.API.Endpoints.Profile.EditCurrentUser
{
    internal sealed class EditCurrentUserCommandHandler(
        IUnitOfWork unitOfWork
    ) : ICommandHandler<EditCurrentUserCommand>
    {
        public async Task<Unit> Handle(EditCurrentUserCommand command, CancellationToken cancellationToken)
        {
            var user = await unitOfWork.Users.FirstOrDefaultAsync(x => x.IdentityId == command.UserId, track: true, cancellationToken)
                ?? throw new InvalidOperationException("کاربر یافت نشد");

            if (!string.IsNullOrWhiteSpace(command.NationalCode))
            {
                var exists = await unitOfWork.Users.FirstOrDefaultAsync(
                    x => x.NationalCode == command.NationalCode &&
                         x.IdentityId != command.UserId, track: true, cancellationToken) is not null;

                if (exists)
                    throw new ConflictDomainException("کد ملی وارد شده قبلاً استفاده شده است.");
            }

            // بروزرسانی اطلاعات پروفایل
            user.UpdateProfile(
                command.Name,
                command.Family,
                command.NationalCode,
                command.Gender,
                command.BirthDate
            );

            // اگر فیلدهای تغییر رمز ارسال شده باشند، رمز عبور را تغییر بده
            if (!string.IsNullOrEmpty(command.CurrentPassword) && !string.IsNullOrEmpty(command.NewPassword))
            {
                var identity = await unitOfWork.Users.GetIdentityByIdAsync(command.UserId)
                    ?? throw new InvalidOperationException("اطلاعات هویتی کاربر یافت نشد");

                var isValid = BCrypt.Net.BCrypt.Verify(command.CurrentPassword, identity.PasswordHash);
                if (!isValid)
                    throw new UnauthorizedDomainException("رمز فعلی نادرست است");

                identity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.NewPassword);
                await unitOfWork.Users.UpdateIdentityAsync(identity);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}