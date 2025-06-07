using BuildingBlocks.Exceptions;
using UserManagement.API.Infrastructure.Data;

namespace UserManagement.API.Endpoints.Admin.RoleManagement.AssignRole;
internal sealed class AssignRoleToUserCommandHandler(
    UserDbContext db,
    IUserRepository userRepository,
    IUnitOfWork uow,
    ILogger<AssignRoleToUserCommandHandler> logger
) : ICommandHandler<AssignRoleToUserCommand>
{
    public async Task<Unit> Handle(AssignRoleToUserCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Assigning role {RoleId} to user {UserId}", command.RoleId, command.UserId);

        // بررسی وجود کاربر
        var user = await userRepository.GetByIdAsync(command.UserId, false, cancellationToken);
        if (user == null)
        {
            logger.LogWarning("User not found: {UserId}", command.UserId);
            throw new NotFoundException("کاربر یافت نشد", $"کاربری با شناسه {command.UserId} یافت نشد");
        }

        // بررسی وجود نقش
        var role = await db.Roles.FirstOrDefaultAsync(r => r.Id == command.RoleId, cancellationToken);
        if (role == null)
        {
            logger.LogWarning("Role not found: {RoleId}", command.RoleId);
            throw new NotFoundException("نقش یافت نشد", $"نقشی با شناسه {command.RoleId} یافت نشد");
        }

        // بررسی عدم تکرار
        var existingAssignment = await db.UserRoles
            .AnyAsync(ur => ur.UserId == command.UserId && ur.RoleId == command.RoleId, cancellationToken);

        if (existingAssignment)
        {
            logger.LogWarning("Role {RoleId} already assigned to user {UserId}", command.RoleId, command.UserId);
            throw new BadRequestException("نقش قبلاً اختصاص داده شده است",
                $"نقش '{role.Name}' قبلاً به این کاربر اختصاص داده شده است");
        }

        // ایجاد اختصاص نقش
        var userRole = new UserRole
        {
            UserId = command.UserId,
            RoleId = command.RoleId,
            AssignedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await db.UserRoles.AddAsync(userRole, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Role {RoleId} ({RoleName}) successfully assigned to user {UserId}",
            command.RoleId, role.Name, command.UserId);

        return Unit.Value;
    }
}
