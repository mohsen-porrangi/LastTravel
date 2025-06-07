using BuildingBlocks.Contracts.Services;
using BuildingBlocks.Messaging.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Common.Interfaces;
using Order.Application.EventHandlers;
using Order.Domain.Contracts;
using Order.Domain.Services;
using Order.Infrastructure.BackgroundServices;
using Order.Infrastructure.Data.Context;
using Order.Infrastructure.Data.Repositories;
using Order.Infrastructure.ExternalServices.Configuration;
using Order.Infrastructure.ExternalServices.Flight;
using Order.Infrastructure.ExternalServices.Train;
using Order.Infrastructure.ExternalServices.UserManagement;
using Order.Infrastructure.ExternalServices.WalletPayment;
using Order.Infrastructure.Services;

namespace Order.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<OrderDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("OrderConnectionString"),
                b => b.MigrationsAssembly(typeof(OrderDbContext).Assembly.FullName)));

        services.AddScoped<IOrderDbContext>(provider => provider.GetService<OrderDbContext>()!);

        // Repositories
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderFlightRepository, OrderFlightRepository>();
        services.AddScoped<IOrderTrainRepository, OrderTrainRepository>();
        services.AddScoped<IOrderTrainCarTransportRepository, OrderTrainCarTransportRepository>(); //  اضافه شد
        services.AddScoped<ISavedPassengerRepository, SavedPassengerRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Domain Services
        services.AddScoped<IOrderNumberGenerator, OrderNumberGenerator>();
        services.AddScoped<OrderPricingService>();

        // Application Services
        // services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<ITicketService, TicketService>();

        // External Services
        services.Configure<ExternalServicesOptions>(configuration.GetSection("ExternalServices"));
        services.AddHttpClient<IUserManagementService, UserManagementServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:UserManagement:BaseUrl"]
                ?? "https://localhost:7072");
        });
        services.AddHttpClient<IWalletPaymentService, WalletPaymentServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:WalletPayment:BaseUrl"]
                ?? "https://localhost:7240");
        });
        services.AddHttpClient<IFlightService, FlightServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:Flight:BaseUrl"]
                ?? "https://localhost:7300");
        });
        services.AddHttpClient<ITrainService, TrainServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:Train:BaseUrl"]
                ?? "https://localhost:7400");
        });

        services.AddScoped<IFlightService, FlightServiceClient>();
        services.AddScoped<ITrainService, TrainServiceClient>();

        // Background Services
        services.AddHostedService<OrderExpirationService>();

        // Messaging
        services.AddMessaging(configuration, typeof(PaymentCompletedEventHandler).Assembly);

        return services;
    }
}