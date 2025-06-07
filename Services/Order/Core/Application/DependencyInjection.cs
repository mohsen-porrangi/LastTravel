using BuildingBlocks.Behaviors;
using BuildingBlocks.Messaging.Extensions;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Common.Behaviors;
using Order.Application.Common.Interfaces;
using Order.Application.Orders.Services;
using System.Reflection;

namespace Order.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // MediatR
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ExceptionHandlingBehavior<,>));
            config.AddOpenBehavior(typeof(TransactionBehavior<,>));
            config.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // AutoMapper
        services.AddAutoMapper(assembly);

        // ✅ Application Services - FIXED ORDER
        services.AddApplicationServices();

        // ✅ Messaging - AFTER services registration
        services.AddMessaging(configuration, assembly);

        return services;
    }

    /// <summary>
    /// Register application services in correct order
    /// </summary>
    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // ✅ Register OrderProcessingService FIRST
        services.AddScoped<OrderProcessingService>();

        // ✅ Register interface mapping AFTER implementation
        services.AddScoped<IOrderService>(provider =>
            provider.GetRequiredService<OrderProcessingService>());

        return services;
    }
}