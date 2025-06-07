using BuildingBlocks.Exceptions;
using UserManagement.API.Infrastructure.Data;

namespace UserManagement.API.Endpoints.Admin.RoleManagement.UnassignRole;
internal sealed class UnassignRoleFromUserCommandHandler(
    UserDbContext db,
    IUserRepository userRepository,
    IUnitOfWork uow,
    ILogger<UnassignRoleFromUserCommandHandler> logger
) : ICommandHandler<UnassignRoleFromUserCommand>
{
    public async Task<Unit> Handle(UnassignRoleFromUserCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Unassigning role {RoleId} from user {UserId}", command.RoleId, command.UserId);

        // بررسی وجود کاربر
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken: cancellationToken);
        if (user == null)
        {
            logger.LogWarning("User not found: {UserId}", command.UserId);
            throw new NotFoundException("کاربر یافت نشد", $"کاربری با شناسه {command.UserId} یافت نشد");
        }

        // یافتن اختصاص نقش
        var userRole = await db.UserRoles
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync(ur => ur.UserId == command.UserId && ur.RoleId == command.RoleId, cancellationToken);

        if (userRole == null)
        {
            logger.LogWarning("Role assignment not found: RoleId {RoleId}, UserId {UserId}", command.RoleId, command.UserId);
            throw new NotFoundException("اختصاص نقش یافت نشد",
                "این نقش به کاربر اختصاص داده نشده است");
        }

        // حذف اختصاص نقش
        db.UserRoles.Remove(userRole);
        await uow.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Role {RoleId} ({RoleName}) successfully unassigned from user {UserId}",
            command.RoleId, userRole.Role.Name, command.UserId);

        return Unit.Value;
    }
}