using PaymentGateway.API.Models;

namespace PaymentGateway.API.Gateways;

public class SandboxGateway : IPaymentGateway
{
    private readonly ILogger<SandboxGateway> _logger;
    private readonly IConfiguration _configuration;

    public SandboxGateway(ILogger<SandboxGateway> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public PaymentGatewayType GatewayType => PaymentGatewayType.Sandbox;

    public Task<PaymentGatewayResult> CreatePaymentAsync(PaymentGatewayRequest request, CancellationToken cancellationToken = default)
    {
        var authority = $"SBX-{Guid.NewGuid():N}";
        var baseUrl = _configuration["PaymentGateways:Sandbox:BaseUrl"] ?? "http://localhost:5000";
        var paymentUrl = $"{baseUrl}/sandbox-payment?authority={authority}&amount={request.Amount}&callback={Uri.EscapeDataString(request.CallbackUrl)}";

        _logger.LogInformation("Sandbox payment created. Authority: {Authority}, Amount: {Amount}", authority, request.Amount);

        return Task.FromResult(new PaymentGatewayResult(true, authority, paymentUrl));
    }

    public Task<PaymentGatewayVerificationResult> VerifyPaymentAsync(PaymentGatewayVerificationRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Status != "OK" && request.Status != "success")
        {
            _logger.LogWarning("Sandbox payment cancelled. Authority: {Authority}, Status: {Status}", request.Authority, request.Status);
            return Task.FromResult(new PaymentGatewayVerificationResult(false, ErrorMessage: "پرداخت لغو شد", ErrorCode: PaymentErrorCode.CanceledByUser));
        }

        var referenceId = $"SBX-REF-{DateTime.Now:yyyyMMddHHmmss}";
        _logger.LogInformation("Sandbox payment verified. Authority: {Authority}, ReferenceId: {ReferenceId}", request.Authority, referenceId);

        return Task.FromResult(new PaymentGatewayVerificationResult(true, referenceId, request.Amount));
    }

    public Task<PaymentGatewayRefundResult> RefundPaymentAsync(PaymentGatewayRefundRequest request, CancellationToken cancellationToken = default)
    {
        var refundTrackingId = $"SBX-REFUND-{DateTime.Now:yyyyMMddHHmmss}";
        _logger.LogInformation("Sandbox refund processed. ReferenceId: {ReferenceId}, RefundTrackingId: {RefundTrackingId}", request.ReferenceId, refundTrackingId);

        return Task.FromResult(new PaymentGatewayRefundResult(true, refundTrackingId));
    }
}