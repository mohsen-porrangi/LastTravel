using Microsoft.Extensions.Logging;
using WalletPayment.Application.Common.Interfaces;
using WalletPayment.Domain.ValueObjects;
using WalletPayment.Domain.Enums;

namespace WalletPayment.Infrastructure.Services;

/// <summary>
/// Payment gateway client implementation
/// </summary>
public class PaymentGatewayClient : IPaymentGatewayClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentGatewayClient> _logger;

    public PaymentGatewayClient(HttpClient httpClient, ILogger<PaymentGatewayClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PaymentResult> CreatePaymentAsync(
        Money amount,
        string description,
        string callbackUrl,
        string? orderId = null,
        PaymentGatewayType gateway = PaymentGatewayType.ZarinPal,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating payment: Amount={Amount}, Gateway={Gateway}, OrderId={OrderId}",
            amount, gateway, orderId);

        try
        {
            if (gateway == PaymentGatewayType.Sandbox)
            {
                return await CreateSandboxPaymentAsync(amount, description, callbackUrl, orderId, cancellationToken);
            }

            // TODO: Implement real ZarinPal integration
            return await CreateMockZarinPalPaymentAsync(amount, description, callbackUrl, orderId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create payment for amount {Amount}", amount);
            return new PaymentResult
            {
                IsSuccessful = false,
                ErrorMessage = "خطا در ایجاد درخواست پرداخت",
                ErrorCode = "GATEWAY_ERROR"
            };
        }
    }

    public async Task<PaymentVerificationResult> VerifyPaymentAsync(
        string authority,
        Money expectedAmount,
        PaymentGatewayType gateway = PaymentGatewayType.ZarinPal,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Verifying payment: Authority={Authority}, Amount={Amount}, Gateway={Gateway}",
            authority, expectedAmount, gateway);

        try
        {
            if (gateway == PaymentGatewayType.Sandbox)
            {
                return await VerifySandboxPaymentAsync(authority, expectedAmount, cancellationToken);
            }

            // TODO: Implement real ZarinPal verification
            return await VerifyMockZarinPalPaymentAsync(authority, expectedAmount, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify payment for authority {Authority}", authority);
            return new PaymentVerificationResult
            {
                IsSuccessful = false,
                IsVerified = false,
                ErrorMessage = "خطا در تایید پرداخت",
                ErrorCode = "VERIFICATION_ERROR"
            };
        }
    }

    public async Task<PaymentStatusResult> GetPaymentStatusAsync(
        string authority,
        PaymentGatewayType gateway = PaymentGatewayType.ZarinPal,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting payment status: Authority={Authority}, Gateway={Gateway}", authority, gateway);

        try
        {
            // Mock implementation for now
            await Task.Delay(100, cancellationToken); // Simulate API call

            return new PaymentStatusResult
            {
                IsSuccessful = true,
                Status = PaymentStatus.Paid,
                Amount = Money.Create(100000, CurrencyCode.IRR),
                CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                PaidAt = DateTime.UtcNow.AddMinutes(-2)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payment status for authority {Authority}", authority);
            return new PaymentStatusResult
            {
                IsSuccessful = false,
                Status = PaymentStatus.Failed,
                ErrorMessage = "خطا در دریافت وضعیت پرداخت"
            };
        }
    }

    #region Private Mock Implementation Methods

    private async Task<PaymentResult> CreateSandboxPaymentAsync(
        Money amount,
        string description,
        string callbackUrl,
        string? orderId,
        CancellationToken cancellationToken)
    {
        await Task.Delay(200, cancellationToken); // Simulate API call

        var authority = Guid.NewGuid().ToString();
        var paymentUrl = $"https://sandbox.zarinpal.com/pg/StartPay/{authority}";

        return new PaymentResult
        {
            IsSuccessful = true,
            Authority = authority,
            PaymentUrl = paymentUrl
        };
    }

    private async Task<PaymentResult> CreateMockZarinPalPaymentAsync(
        Money amount,
        string description,
        string callbackUrl,
        string? orderId,
        CancellationToken cancellationToken)
    {
        await Task.Delay(300, cancellationToken); // Simulate API call

        // Mock successful response
        var authority = Guid.NewGuid().ToString();
        var paymentUrl = $"https://www.zarinpal.com/pg/StartPay/{authority}";

        return new PaymentResult
        {
            IsSuccessful = true,
            Authority = authority,
            PaymentUrl = paymentUrl
        };
    }

    private async Task<PaymentVerificationResult> VerifySandboxPaymentAsync(
        string authority,
        Money expectedAmount,
        CancellationToken cancellationToken)
    {
        await Task.Delay(200, cancellationToken); // Simulate API call

        return new PaymentVerificationResult
        {
            IsSuccessful = true,
            IsVerified = true,
            ReferenceId = Random.Shared.Next(100000, 999999).ToString(),
            ActualAmount = expectedAmount,
            VerificationDate = DateTime.UtcNow
        };
    }

    private async Task<PaymentVerificationResult> VerifyMockZarinPalPaymentAsync(
        string authority,
        Money expectedAmount,
        CancellationToken cancellationToken)
    {
        await Task.Delay(300, cancellationToken); // Simulate API call

        return new PaymentVerificationResult
        {
            IsSuccessful = true,
            IsVerified = true,
            ReferenceId = Random.Shared.Next(100000, 999999).ToString(),
            ActualAmount = expectedAmount,
            VerificationDate = DateTime.UtcNow
        };
    }

    #endregion
}