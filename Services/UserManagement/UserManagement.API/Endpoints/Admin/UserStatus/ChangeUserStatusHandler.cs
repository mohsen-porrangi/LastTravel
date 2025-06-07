namespace UserManagement.API.Endpoints.Admin.UserStatus;

internal sealed class ChangeUserStatusCommandHandler(
    IUserRepository repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<ChangeUserStatusCommand>
{
    public async Task<Unit> Handle(ChangeUserStatusCommand command, CancellationToken cancellationToken)
    {
        var user = await repository.FirstOrDefaultWithIncludesAsync(
            condition => condition.Id.Equals(command.Id), 
            i => i.Include(include => include.MasterIdentity) 
            ,false, 
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("کاربر یافت نشد");

        if (command.IsActive)
            user.Activate();
        else
            user.Deactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}