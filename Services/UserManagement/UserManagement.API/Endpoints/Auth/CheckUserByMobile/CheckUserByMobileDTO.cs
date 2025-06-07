namespace UserManagement.API.Endpoints.Auth.CheckUserByMobile
{

    public record CheckUserByMobileQuery(string Mobile) : IQuery<CheckUserByMobileResponse>;

    public record CheckUserByMobileResponse(bool Exists);
}
