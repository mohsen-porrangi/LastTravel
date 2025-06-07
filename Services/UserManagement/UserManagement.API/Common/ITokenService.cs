namespace UserManagement.API.Common
{
    public interface ITokenService
    {
        string GenerateToken(User user, IEnumerable<string> permissionCodes);
        bool ValidateToken(string token);
    }
}
