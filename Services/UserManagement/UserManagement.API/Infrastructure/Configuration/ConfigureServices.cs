using BuildingBlocks.Behaviors;
using BuildingBlocks.Contracts.Security;
using BuildingBlocks.Contracts.Services;
using BuildingBlocks.Messaging.Extensions;
using System.Reflection;
using UserManagement.API.Endpoints.AccessControl.Services;
using UserManagement.API.Infrastructure.Data;
using UserManagement.API.Services;

namespace UserManagement.API.Infrastructure.Configuration
{
    public static class ConfigureServices
    {
        private static Assembly assembly = typeof(Program).Assembly;

        //public static IServiceCollection ConfigureSqlServer(this IServiceCollection services, IConfiguration configuration)
        //{
        //    //  Configuration binding improvements
        //    var connectionString = configuration.GetConnectionString("UserConnectionString")
        //        ?? throw new InvalidOperationException("UserConnectionString not found in configuration");

        //    services.AddDbContext<AppDbContext>(options =>
        //        options.UseSqlServer(connectionString)
        //               .EnableSensitiveDataLogging(false) // Production safety
        //               .EnableDetailedErrors(false));

        //    return services;
        //}
        public static void ConfigureSqlServer(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddDbContext<UserDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("UserConnectionString")));
        }

        //public static IServiceCollection ConfigureMediatR(this IServiceCollection services, IConfiguration configuration)
        //{
        //    services.AddMediatR(config =>
        //    {
        //        config.RegisterServicesFromAssembly(assembly);
        //        config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        //        config.AddOpenBehavior(typeof(LoggingBehavior<,>));
        //        config.AddOpenBehavior(typeof(ExceptionHandlingBehavior<,>));
        //    });

        //    return services;
        //}
        public static void ConfigureMediatR(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(assembly);
                config.AddOpenBehavior(typeof(ValidationBehavior<,>));
                config.AddOpenBehavior(typeof(LoggingBehavior<,>));
                config.AddOpenBehavior(typeof(ExceptionHandlingBehavior<,>));
            });
        }
        //public static IServiceCollection ConfigureService(this IServiceCollection services, IConfiguration configuration)
        //{
        //    //  Keyed services for better organization
        //    services.AddKeyedScoped<IUserRepository, UserRepository>("primary");
        //    services.AddKeyedScoped<IUnitOfWork, UnitOfWork>("primary");

        //    // Core Services
        //    services.AddScoped<IOtpService, OtpService>();
        //    services.AddScoped<IPermissionService, UserPermissionService>();
        //    services.AddScoped<IUserManagementService, Services.UserManagementService>();

        //    //  Modern service configuration
        //    services.AddSingleton<ITemporaryRegistrationService, TemporaryRegistrationService>();
        //    services.AddScoped<IPermissionManagementService, PermissionManagementService>();

        //    //  Memory cache with options
        //    services.AddMemoryCache(options =>
        //    {
        //        options.SizeLimit = 1000; // Limit cache size
        //        options.CompactionPercentage = 0.2; // Compact when 80% full
        //    });

        //    //  Messaging with keyed services
        //    services.AddKeyedSingleton<BuildingBlocks.Messaging.Contracts.IMessageBus, BuildingBlocks.Messaging.InMemory.InMemoryMessageBus>("memory");
        //    services.AddKeyedSingleton<BuildingBlocks.Messaging.Contracts.IMessageBusSubscriptionsManager, BuildingBlocks.Messaging.InMemory.InMemoryMessageBusSubscriptionsManager>("memory");

        //    //  TODO: Wallet Service باید implement شود
        //    // services.AddScoped<IWalletPaymentService, WalletPaymentService>();

        //    //  Mock Implementation تا Wallet Service اصلی پیاده‌سازی شود
        //    // services.AddScoped<IWalletPaymentService, ModernMockWalletPaymentService>();
        //    services.AddMessaging(configuration, assembly);
        //    return services;
        //}
        public static void ConfigureService(this IServiceCollection services, IConfiguration configuration)
        {
            var walletBaseAddress = configuration.GetSection("WalletService:BaseAddress").Value
                                    ?? throw new InvalidOperationException("WalletService:BaseAddress not configured");

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IPermissionService, UserPermissionService>();
            services.AddScoped<IUserManagementService, Services.UserManagementService>();
            services.AddScoped<ITemporaryRegistrationService, TemporaryRegistrationService>();
            services.AddScoped<IWalletPaymentService, WalletPaymentService>();

            services.AddHttpClient<IWalletPaymentService, WalletPaymentService>(client =>
            {
                client.BaseAddress = new Uri(walletBaseAddress);
            });
            //  services.AddSingleton<BuildingBlocks.Messaging.Contracts.IMessageBus, BuildingBlocks.Messaging.InMemory.InMemoryMessageBus>();
            //  services.AddSingleton<BuildingBlocks.Messaging.Contracts.IMessageBusSubscriptionsManager, BuildingBlocks.Messaging.InMemory.InMemoryMessageBusSubscriptionsManager>();
            //  Memory cache with options
            services.AddMemoryCache();

            //  Messaging with keyed services
            //    services.AddKeyedSingleton<BuildingBlocks.Messaging.Contracts.IMessageBus, BuildingBlocks.Messaging.InMemory.InMemoryMessageBus>("memory");
            //    services.AddKeyedSingleton<BuildingBlocks.Messaging.Contracts.IMessageBusSubscriptionsManager, BuildingBlocks.Messaging.InMemory.InMemoryMessageBusSubscriptionsManager>("memory");
            services.AddMessaging(configuration, assembly);

        }

    }
}