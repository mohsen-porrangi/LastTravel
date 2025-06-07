namespace UserManagement.API.Endpoints.Auth.RegisterUser;

public class RegisterUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/register", async (
            RegisterUserCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(command, ct);
            return Results.NoContent();
        })
            .WithTags("Auth")
            .WithName("UserRegister")
            .WithDescription("ثبت‌نام کاربر جدید با شماره موبایل و رمز عبور")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .AllowAnonymous();
    }
}
