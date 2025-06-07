using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WalletPayment.Application.Common.Interfaces;
using WalletPayment.Application.Features.Refunds;
using WalletPayment.Domain.Common.Contracts;
using WalletPayment.Domain.DomainServices;
using WalletPayment.Infrastructure.BackgroundServices;
using WalletPayment.Infrastructure.Persistence;
using WalletPayment.Infrastructure.Persistence.Context;
using WalletPayment.Infrastructure.Services;

namespace WalletPayment.Infrastructure;

/// <summary>
/// Infrastructure layer dependency injection
/// ✅ Fixed: Complete and organized DI configuration
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database Configuration
        services.AddDatabase(configuration);

        // Repository & UoW Pattern
        services.AddRepositories();

        // Domain Services
        services.AddDomainServices();

        // Application Services
        services.AddApplicationServices();

        // External Services
        services.AddExternalServices();

        // Background Services
        services.AddBackgroundServices();

        return services;
    }

    /// <summary>
    /// Configure database and DbContext
    /// </summary>
    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<WalletDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("WalletConnectionString"),
                b => b.MigrationsAssembly(typeof(WalletDbContext).Assembly.FullName));

            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });

        // Register DbContext as interface
        services.AddScoped<IWalletDbContext>(provider =>
            provider.GetRequiredService<WalletDbContext>());

        return services;
    }

    /// <summary>
    /// Configure repositories and Unit of Work
    /// </summary>
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Note: Individual repositories are handled by UnitOfWork lazy loading
        // This follows the pattern already implemented in UnitOfWork.cs

        return services;
    }

    /// <summary>
    /// Configure domain services
    /// </summary>
    private static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<ITransactionDomainService, TransactionDomainService>();
        services.AddScoped<IFeeCalculationService, FeeCalculationService>();

        return services;
    }

    /// <summary>
    /// Configure application services
    /// </summary>
    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // ✅ REMOVED - CurrentUserService registered in API layer now
        // Application-specific services
        services.AddScoped<IRefundService, RefundService>();

        return services;
    }

    /// <summary>
    /// Configure external services
    /// </summary>
    private static IServiceCollection AddExternalServices(this IServiceCollection services)
    {
        // HTTP Clients
        services.AddHttpClient<IPaymentGatewayClient, PaymentGatewayClient>();

        // Currency Exchange Service (Mock for now)
        services.AddScoped<ICurrencyExchangeService, CurrencyExchangeService>();

        // Additional external services can be added here
        // services.AddHttpClient<IUserManagementClient, UserManagementClient>();

        return services;
    }

    /// <summary>
    /// Configure background services
    /// </summary>
    private static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<AccountSnapshotBackgroundService>();
        services.AddHostedService<CreditDueDateCheckingService>();

        return services;
    }
}