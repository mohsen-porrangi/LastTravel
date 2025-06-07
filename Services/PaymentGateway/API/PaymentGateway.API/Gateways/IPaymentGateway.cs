using PaymentGateway.API.Models;

namespace PaymentGateway.API.Gateways;

public interface IPaymentGateway
{
    PaymentGatewayType GatewayType { get; }
    Task<PaymentGatewayResult> CreatePaymentAsync(PaymentGatewayRequest request, CancellationToken cancellationToken = default);
    Task<PaymentGatewayVerificationResult> VerifyPaymentAsync(PaymentGatewayVerificationRequest request, CancellationToken cancellationToken = default);
    Task<PaymentGatewayRefundResult> RefundPaymentAsync(PaymentGatewayRefundRequest request, CancellationToken cancellationToken = default);
}

public record PaymentGatewayRequest(
    decimal Amount,
    CurrencyCode Currency,
    string Description,
    string CallbackUrl,
    string? MobileNumber = null,
    string? Email = null,
    Dictionary<string, string>? Metadata = null);

public record PaymentGatewayResult(
    bool IsSuccessful,
    string? Authority = null,
    string? PaymentUrl = null,
    string? ErrorMessage = null,
    PaymentErrorCode? ErrorCode = null);

public record PaymentGatewayVerificationRequest(
    string Authority,
    string Status,
    decimal Amount);

public record PaymentGatewayVerificationResult(
    bool IsSuccessful,
    string? ReferenceId = null,
    decimal Amount = 0,
    string? ErrorMessage = null,
    PaymentErrorCode? ErrorCode = null);

public record PaymentGatewayRefundRequest(
    string ReferenceId,
    decimal Amount,
    string Reason);

public record PaymentGatewayRefundResult(
    bool IsSuccessful,
    string? RefundTrackingId = null,
    string? ErrorMessage = null,
    PaymentErrorCode? ErrorCode = null);