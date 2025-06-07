namespace UserManagement.API.Endpoints.Auth.RefreshToken;

internal sealed class RefreshTokenCommandHandler(
    IUnitOfWork uow,
    ITokenService tokenService
) : ICommandHandler<RefreshTokenCommand, (string AccessToken, string RefreshToken)>
{
    public async Task<(string AccessToken, string RefreshToken)> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var identity = await uow.Users.GetIdentityByRefreshTokenAsync(command.RefreshToken);
        if (identity == null)
            throw new UnauthorizedAccessException("توکن نامعتبر است");

        var user = await uow.Users.GetUserByIdentityIdAsync(identity.Id)
                   ?? throw new UnauthorizedAccessException("کاربر یافت نشد یا غیرفعال است");

        var permissions = await uow.Users.GetUserPermissionsAsync(user.Id);

        // invalidate old token
        await uow.Users.InvalidateRefreshTokenAsync(identity.Id);

        // generate new refresh token
        var newRefreshToken = Guid.NewGuid().ToString();
        await uow.Users.StoreRefreshTokenAsync(identity.Id, newRefreshToken);

        await uow.SaveChangesAsync(cancellationToken);

        var accessToken = tokenService.GenerateToken(user, permissions);

        return (accessToken, newRefreshToken);
    }
}