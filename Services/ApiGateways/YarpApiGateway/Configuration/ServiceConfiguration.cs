namespace YarpApiGateway.Configuration;

public class ServiceConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string SwaggerPath { get; set; } = "/swagger/v1/swagger.json";
    public string HealthPath { get; set; } = "/health";
    public List<string> Endpoints { get; set; } = new();
}

public class ServicesOptions
{
    public const string SectionName = "Services";
    public Dictionary<string, ServiceConfiguration> Services { get; set; } = new();
}

public static class ServiceRegistry
{
    private static readonly Dictionary<string, ServiceConfiguration> _services = new()
    {
        ["user"] = new ServiceConfiguration
        {
            Name = "user",
            DisplayName = "User Management Service",
            Icon = "👤",
            BaseUrl = "https://localhost:7072",
            SwaggerPath = "/swagger/v1/swagger.json",
            HealthPath = "/health",
            Endpoints = new() { "/api/auth/*", "/api/users/*", "/api/roles/*" }
        },
        ["wallet"] = new ServiceConfiguration
        {
            Name = "wallet",
            DisplayName = "Wallet Payment Service",
            Icon = "💳",
            BaseUrl = "https://localhost:7240",
            SwaggerPath = "/swagger/v1/swagger.json",
            HealthPath = "/health",
            Endpoints = new() { "/api/wallets/*", "/api/payments/*", "/api/transactions/*", "/api/currency/*", "/api/accounts/*", "/api/bank-accounts/*" }
        },
        ["order"] = new ServiceConfiguration
        {
            Name = "order",
            DisplayName = "Order Management Service",
            Icon = "📦",
            BaseUrl = "https://localhost:60102",
            SwaggerPath = "/swagger/v1/swagger.json",
            HealthPath = "/health",
            Endpoints = new() { "/api/orders/*", "/api/passengers/*", "/api/tickets/*" }
        }
    };

    public static IReadOnlyDictionary<string, ServiceConfiguration> Services => _services;

    public static ServiceConfiguration? GetService(string name)
    {
        return _services.TryGetValue(name.ToLowerInvariant(), out var service) ? service : null;
    }

    public static void AddService(string name, ServiceConfiguration config)
    {
        _services[name.ToLowerInvariant()] = config;
    }
}