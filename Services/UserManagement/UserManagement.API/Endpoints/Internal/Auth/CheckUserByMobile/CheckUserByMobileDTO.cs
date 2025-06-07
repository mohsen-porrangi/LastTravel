namespace UserManagement.API.Endpoints.Auth
{

    public record CheckUserByMobileQuery(string Mobile) : IQuery<CheckUserByMobileResponse>;

    public record CheckUserByMobileResponse(bool Exists);
}
