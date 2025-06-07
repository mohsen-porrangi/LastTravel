using Order.API.Middleware;

namespace Order.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseOrderMiddlewares(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseCurrentUser();
        app.UseAuthorization();

        return app;
    }
}