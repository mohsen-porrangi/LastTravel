using BuildingBlocks.Contracts;
using Order.API.Services;

namespace Order.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCurrentUserService(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        return services;
    }
}