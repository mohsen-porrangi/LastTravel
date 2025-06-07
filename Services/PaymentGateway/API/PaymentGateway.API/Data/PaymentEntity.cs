using PaymentGateway.API.Models;

namespace PaymentGateway.API.Data;

public class Payment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public CurrencyCode Currency { get; set; }
    public PaymentGatewayType GatewayType { get; set; }
    public string Description { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string? Authority { get; set; }
    public string? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? CallbackUrl { get; set; }
    public string? OrderId { get; set; }
    public string? ErrorMessage { get; set; }
    public PaymentErrorCode? ErrorCode { get; set; }
    public string? RefundTrackingId { get; set; }
    public string? AdditionalData { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}

public enum PaymentStatus
{
    Pending = 1,
    Processing = 2,
    Paid = 3,
    Verified = 4,
    Failed = 5,
    Cancelled = 6,
    Refunded = 7
}