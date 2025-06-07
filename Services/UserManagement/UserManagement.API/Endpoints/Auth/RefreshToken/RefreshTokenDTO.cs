namespace UserManagement.API.Endpoints.Auth.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : ICommand<(string AccessToken, string RefreshToken)>;

