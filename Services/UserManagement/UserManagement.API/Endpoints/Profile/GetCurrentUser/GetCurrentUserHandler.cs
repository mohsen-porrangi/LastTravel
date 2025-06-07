using BuildingBlocks.Utils;

namespace UserManagement.API.Endpoints.Profile.GetCurrentUser
{
    internal sealed class GetCurrentUserQueryHandler(IUserRepository repository)
     : IQueryHandler<GetCurrentUserQuery, GetCurrentUserResult>
    {
        public async Task<GetCurrentUserResult> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
        {
            var user = await repository.FirstOrDefaultWithIncludesAsync(x => x.IdentityId == query.IdentityId, include: q => q.Include(u => u.MasterIdentity), track: false)
                       ?? throw new InvalidOperationException("کاربر یافت نشد");

            var identity = user.MasterIdentity;

            return new GetCurrentUserResult(
                user.Id,
                user.Name,
                user.Family,
                identity.Email,
                identity.Mobile,
                user.NationalCode,
                user.BirthDate,
                (int?)user.Gender,
                user?.Gender?.GetEnumDescription(),
                identity.IsActive
            );
        }
    }
    internal static class EnumExtensions
    {
        public static string? GetEnumDescriptionOrNull(this Enum? value)
        {
            return value == null ? null : value.GetEnumDescription();
        }
    }
}
