namespace UserManagement.API.Endpoints.Admin.GetUserByCondition
{
    public record GetUserByConditionQuery(
        string? Name = null,
        string? Family = null,
        string? Mobile = null,
        bool? IsActive = null,
        string? NationalCode = null,
        int Page = 1,
        int PageSize = 10
    ) : IQuery<GetUserByConditionResult>;

    public record GetUserByConditionResult(
        IEnumerable<UserDto> Users,
        int TotalCount,
        int Page,
        int PageSize
    );

    public record UserDto(
        Guid Id,
        string Name,
        string Family,
        string Email,
        string? Mobile,
        bool IsActive,
        string? NationalCode
    );
}