using System.Reflection;
using BuildingBlocks.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using WalletPayment.Application.Common.Behaviors;

namespace WalletPayment.Application;

/// <summary>
/// Application layer dependency injection configuration
/// ✅ Fixed: Complete application layer DI
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // MediatR Registration with all behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Pipeline behaviors in order of execution
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ExceptionHandlingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        // FluentValidation Registration
        services.AddValidatorsFromAssembly(assembly);

        // Application Services
        services.AddApplicationServices();

        return services;
    }

    /// <summary>
    /// Register application-specific services
    /// </summary>
    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Add any application-specific services here
        // Example: services.AddScoped<IReportService, ReportService>();

        return services;
    }
}