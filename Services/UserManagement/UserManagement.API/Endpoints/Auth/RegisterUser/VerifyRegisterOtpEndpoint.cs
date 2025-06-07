using UserManagement.API.Endpoints.Auth.Login;

namespace UserManagement.API.Endpoints.Auth.RegisterUser;

public class VerifyRegisterOtpEndpointRemove : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/register/verify-otp", async (
            VerifyRegisterOtpCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Ok(result);
        })
          .WithTags("Auth")
          .WithName("UserRegisterVerifyOtp")
          .WithDescription("تأیید کد یکبار مصرف برای فعال‌سازی حساب کاربری")
          .Produces<LoginResult>(StatusCodes.Status200OK)
          .Produces(StatusCodes.Status401Unauthorized)
          .ProducesProblem(StatusCodes.Status400BadRequest)
          .AllowAnonymous();
    }
}
