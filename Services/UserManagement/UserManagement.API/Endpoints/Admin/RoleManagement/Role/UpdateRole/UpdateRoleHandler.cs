using UserManagement.API.Infrastructure.Data;

namespace UserManagement.API.Endpoints.Admin.RoleManagement.Role.UpdateRole;

internal sealed class UpdateRoleCommandHandler(UserDbContext db, IUnitOfWork uow)
    : ICommandHandler<UpdateRoleCommand>
{
    public async Task<Unit> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        var role = await db.Roles.FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken)
                   ?? throw new InvalidOperationException("نقش یافت نشد");

        role.Name = command.Name;

        await uow.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
