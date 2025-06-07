using Microsoft.EntityFrameworkCore;
using PaymentGateway.API.Data;
using PaymentGateway.API.Gateways;
using PaymentGateway.API.Models;
using System.Text.Json;

namespace PaymentGateway.API.Services;

public interface IPaymentService
{
    Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);
    Task<VerifyPaymentResponse> VerifyPaymentAsync(VerifyPaymentRequest request, CancellationToken cancellationToken = default);
    Task<RefundPaymentResponse> RefundPaymentAsync(string paymentId, RefundPaymentRequest request, CancellationToken cancellationToken = default);
    Task<Payment?> GetPaymentAsync(string paymentId, CancellationToken cancellationToken = default);
}

public class PaymentService : IPaymentService
{
    private readonly PaymentDbContext _dbContext;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(PaymentDbContext dbContext, IPaymentGatewayFactory gatewayFactory, ILogger<PaymentService> logger)
    {
        _dbContext = dbContext;
        _gatewayFactory = gatewayFactory;
        _logger = logger;
    }

    public async Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create payment record
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Amount = request.Amount,
                Currency = request.Currency,
                GatewayType = request.GatewayType,
                Description = request.Description,
                Status = PaymentStatus.Pending,
                CallbackUrl = request.CallbackUrl,
                OrderId = request.OrderId,
                AdditionalData = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null
            };

            await _dbContext.Payments.AddAsync(payment, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Create payment with gateway
            var gateway = _gatewayFactory.CreateGateway(request.GatewayType);
            var gatewayRequest = new PaymentGatewayRequest(
                request.Amount,
                request.Currency,
                request.Description,
                request.CallbackUrl,
                request.MobileNumber,
                request.Email,
                request.Metadata
            );

            var result = await gateway.CreatePaymentAsync(gatewayRequest, cancellationToken);

            if (result.IsSuccessful)
            {
                payment.Authority = result.Authority;
                payment.Status = PaymentStatus.Processing;
                _logger.LogInformation("Payment created successfully. PaymentId: {PaymentId}, Authority: {Authority}", payment.Id, result.Authority);
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.ErrorMessage = result.ErrorMessage;
                payment.ErrorCode = result.ErrorCode;
                _logger.LogWarning("Payment creation failed. PaymentId: {PaymentId}, Error: {Error}", payment.Id, result.ErrorMessage);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new PaymentResponse
            {
                IsSuccessful = result.IsSuccessful,
                PaymentUrl = result.PaymentUrl,
                Authority = result.Authority,
                ErrorMessage = result.ErrorMessage,
                ErrorCode = result.ErrorCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while creating payment for user {UserId}", request.UserId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = "خطای سیستمی در ایجاد پرداخت",
                ErrorCode = PaymentErrorCode.Unknown
            };
        }
    }

    public async Task<VerifyPaymentResponse> VerifyPaymentAsync(VerifyPaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find payment record
            var payment = await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.Authority == request.Authority, cancellationToken);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found for authority: {Authority}", request.Authority);
                return new VerifyPaymentResponse
                {
                    IsSuccessful = false,
                    ErrorMessage = "پرداخت یافت نشد",
                    ErrorCode = PaymentErrorCode.AuthorityNotFound
                };
            }

            // Verify with gateway
            var gateway = _gatewayFactory.CreateGateway(payment.GatewayType);
            var gatewayRequest = new PaymentGatewayVerificationRequest(request.Authority, request.Status, request.Amount);
            var result = await gateway.VerifyPaymentAsync(gatewayRequest, cancellationToken);

            if (result.IsSuccessful)
            {
                payment.Status = PaymentStatus.Verified;
                payment.ReferenceId = result.ReferenceId;
                payment.PaidAt = DateTime.UtcNow;
                payment.VerifiedAt = DateTime.UtcNow;
                _logger.LogInformation("Payment verified successfully. PaymentId: {PaymentId}, ReferenceId: {ReferenceId}", payment.Id, result.ReferenceId);
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.ErrorMessage = result.ErrorMessage;
                payment.ErrorCode = result.ErrorCode;
                _logger.LogWarning("Payment verification failed. PaymentId: {PaymentId}, Error: {Error}", payment.Id, result.ErrorMessage);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new VerifyPaymentResponse
            {
                IsSuccessful = result.IsSuccessful,
                ReferenceId = result.ReferenceId,
                Amount = result.Amount,
                ErrorMessage = result.ErrorMessage,
                ErrorCode = result.ErrorCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while verifying payment with authority {Authority}", request.Authority);
            return new VerifyPaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = "خطای سیستمی در تأیید پرداخت",
                ErrorCode = PaymentErrorCode.Unknown
            };
        }
    }

    public async Task<RefundPaymentResponse> RefundPaymentAsync(string paymentId, RefundPaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(paymentId, out var paymentGuid))
            {
                return new RefundPaymentResponse
                {
                    IsSuccessful = false,
                    ErrorMessage = "شناسه پرداخت نامعتبر است",
                    ErrorCode = PaymentErrorCode.AuthorityNotFound
                };
            }

            var payment = await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentGuid, cancellationToken);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found for refund: {PaymentId}", paymentId);
                return new RefundPaymentResponse
                {
                    IsSuccessful = false,
                    ErrorMessage = "پرداخت یافت نشد",
                    ErrorCode = PaymentErrorCode.AuthorityNotFound
                };
            }

            if (payment.Status != PaymentStatus.Verified)
            {
                _logger.LogWarning("Payment is not in verified status for refund: {PaymentId}, Status: {Status}", paymentId, payment.Status);
                return new RefundPaymentResponse
                {
                    IsSuccessful = false,
                    ErrorMessage = "پرداخت قابل استرداد نیست",
                    ErrorCode = PaymentErrorCode.RefundNotAllowed
                };
            }

            // Refund with gateway
            var gateway = _gatewayFactory.CreateGateway(payment.GatewayType);
            var gatewayRequest = new PaymentGatewayRefundRequest(payment.ReferenceId!, request.Amount, request.Reason);
            var result = await gateway.RefundPaymentAsync(gatewayRequest, cancellationToken);

            if (result.IsSuccessful)
            {
                payment.Status = PaymentStatus.Refunded;
                payment.RefundTrackingId = result.RefundTrackingId;
                _logger.LogInformation("Payment refunded successfully. PaymentId: {PaymentId}, RefundTrackingId: {RefundTrackingId}", payment.Id, result.RefundTrackingId);
            }
            else
            {
                _logger.LogWarning("Payment refund failed. PaymentId: {PaymentId}, Error: {Error}", payment.Id, result.ErrorMessage);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new RefundPaymentResponse
            {
                IsSuccessful = result.IsSuccessful,
                RefundTrackingId = result.RefundTrackingId,
                ErrorMessage = result.ErrorMessage,
                ErrorCode = result.ErrorCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while refunding payment {PaymentId}", paymentId);
            return new RefundPaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = "خطای سیستمی در استرداد پرداخت",
                ErrorCode = PaymentErrorCode.Unknown
            };
        }
    }

    public async Task<Payment?> GetPaymentAsync(string paymentId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(paymentId, out var paymentGuid))
            return null;

        return await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.Id == paymentGuid, cancellationToken);
    }
}