using UserManagement.API.Infrastructure.Data;

namespace UserManagement.API.Endpoints.Admin.RoleManagement.Role.DeleteRole;

internal sealed class DeleteRoleCommandHandler(UserDbContext db, IUnitOfWork uow)
    : ICommandHandler<DeleteRoleCommand>
{
    public async Task<Unit> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        var role = await db.Roles.FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken)
                   ?? throw new InvalidOperationException("نقش یافت نشد");

        db.Roles.Remove(role);
        await uow.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
