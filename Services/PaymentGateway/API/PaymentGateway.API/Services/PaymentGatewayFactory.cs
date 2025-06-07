using PaymentGateway.API.Gateways;
using PaymentGateway.API.Models;

namespace PaymentGateway.API.Services;

public interface IPaymentGatewayFactory
{
    IPaymentGateway CreateGateway(PaymentGatewayType gatewayType);
}

public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentGatewayFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPaymentGateway CreateGateway(PaymentGatewayType gatewayType)
    {
        return gatewayType switch
        {
            PaymentGatewayType.ZarinPal => _serviceProvider.GetRequiredService<ZarinPalGateway>(),
            PaymentGatewayType.Sandbox => _serviceProvider.GetRequiredService<SandboxGateway>(),
            _ => throw new ArgumentException($"Payment gateway {gatewayType} is not supported")
        };
    }
}