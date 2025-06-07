using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.API.Models;

public record CreatePaymentRequest
{
    [Required]
    public Guid UserId { get; init; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; init; }

    [Required]
    public CurrencyCode Currency { get; init; }

    [Required]
    public string Description { get; init; } = string.Empty;

    [Required]
    public PaymentGatewayType GatewayType { get; init; }

    [Required]
    public string CallbackUrl { get; init; } = string.Empty;

    public string? OrderId { get; init; }
    public string? MobileNumber { get; init; }
    public string? Email { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}

public record PaymentResponse
{
    public bool IsSuccessful { get; init; }
    public string? PaymentUrl { get; init; }
    public string? Authority { get; init; }
    public string? ErrorMessage { get; init; }
    public PaymentErrorCode? ErrorCode { get; init; }
}

public record VerifyPaymentRequest
{
    [Required]
    public string Authority { get; init; } = string.Empty;

    [Required]
    public string Status { get; init; } = string.Empty;

    [Required]
    public decimal Amount { get; init; }
}

public record VerifyPaymentResponse
{
    public bool IsSuccessful { get; init; }
    public string? ReferenceId { get; init; }
    public decimal Amount { get; init; }
    public string? ErrorMessage { get; init; }
    public PaymentErrorCode? ErrorCode { get; init; }
}

public record RefundPaymentRequest
{
    [Required]
    public string ReferenceId { get; init; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; init; }

    [Required]
    public string Reason { get; init; } = string.Empty;
}

public record RefundPaymentResponse
{
    public bool IsSuccessful { get; init; }
    public string? RefundTrackingId { get; init; }
    public string? ErrorMessage { get; init; }
    public PaymentErrorCode? ErrorCode { get; init; }
}

public enum CurrencyCode
{
    IRR = 1,
    USD = 2,
    EUR = 3,
    GBP = 4,
    AED = 5
}

public enum PaymentGatewayType
{
    ZarinPal = 1,
    PayIr = 2,
    NextPay = 3,
    Zibal = 4,
    Sandbox = 99
}

public enum PaymentErrorCode
{
    Unknown = 0,
    InvalidAmount = 1,
    InvalidCurrency = 2,
    DuplicateTransaction = 3,
    AuthorityNotFound = 4,
    ExpiredTransaction = 5,
    InvalidIpAddress = 6,
    MerchantNotFound = 7,
    ConnectionFailed = 8,
    GatewayError = 9,
    RefundNotAllowed = 10,
    CanceledByUser = 11
}