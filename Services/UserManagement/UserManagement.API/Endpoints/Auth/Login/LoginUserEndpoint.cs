namespace UserManagement.API.Endpoints.Auth.Login;

public class LoginUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/login", async (
            LoginUserCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("UserLogin")
        .WithDescription("ورود کاربر با شماره موبایل و رمز عبور یا OTP")
        .Produces<LoginResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithTags("Auth");
    }
}