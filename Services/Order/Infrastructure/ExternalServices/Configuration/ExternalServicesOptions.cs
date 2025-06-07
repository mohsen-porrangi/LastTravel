namespace Order.Infrastructure.ExternalServices.Configuration;

public class ExternalServicesOptions
{
    public ServiceEndpoint UserManagement { get; set; } = new();
    public ServiceEndpoint WalletPayment { get; set; } = new();
    public ServiceEndpoint FlightService { get; set; } = new();
    public ServiceEndpoint TrainService { get; set; } = new();
}

public class ServiceEndpoint
{
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
}