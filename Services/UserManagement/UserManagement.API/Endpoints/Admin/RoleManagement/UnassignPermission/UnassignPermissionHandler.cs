using UserManagement.API.Infrastructure.Data;

namespace UserManagement.API.Endpoints.Admin.RoleManagement.UnassignPermission;

internal sealed class UnassignPermissionCommandHandler(UserDbContext db, IUnitOfWork uow)
    : ICommandHandler<UnassignPermissionCommand>
{
    public async Task<Unit> Handle(UnassignPermissionCommand command, CancellationToken cancellationToken)
    {
        var entity = await db.RolePermissions
            .FirstOrDefaultAsync(rp =>
                rp.RoleId == command.RoleId &&
                rp.PermissionId == command.PermissionId,
                cancellationToken);

        if (entity is not null)
        {
            db.RolePermissions.Remove(entity);
            await uow.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
