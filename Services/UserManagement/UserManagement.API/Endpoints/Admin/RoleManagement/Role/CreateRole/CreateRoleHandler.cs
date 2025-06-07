using UserManagement.API.Infrastructure.Data;

namespace UserManagement.API.Endpoints.Admin.RoleManagement.Role.CreateRole;

internal sealed class CreateRoleCommandHandler(UserDbContext db, IUnitOfWork uow)
    : ICommandHandler<CreateRoleCommand, int>
{
    public async Task<int> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var exists = await db.Roles.AnyAsync(r => r.Name == command.Name, cancellationToken);
        if (exists)
            throw new InvalidOperationException("نقشی با این نام قبلاً وجود دارد");

        var role = new Infrastructure.Data.Models.Role
        {
            Name = command.Name
        };

        await db.Roles.AddAsync(role, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return role.Id;
    }
}
