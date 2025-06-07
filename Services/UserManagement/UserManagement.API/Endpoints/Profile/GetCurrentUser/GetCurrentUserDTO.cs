namespace UserManagement.API.Endpoints.Profile.GetCurrentUser
{

    public record GetCurrentUserQuery(Guid IdentityId) : IQuery<GetCurrentUserResult>;

    public record GetCurrentUserResult(
        Guid Id,
        string? Name,
        string? Family,
        string? Email,
        string? Mobile,
        string? NationalId,
        DateTime? BirthDate,
        int? Gender,
        string? GenderDescription,
        bool IsActive
    );
}
