using BuildingBlocks.Messaging.Events;
using PaymentGateway.API.Models;

namespace PaymentGateway.API.Events;

public record PaymentCreatedEvent : IntegrationEvent
{
    public PaymentCreatedEvent(Guid paymentId, Guid userId, decimal amount, CurrencyCode currency, string? orderId)
    {
        PaymentId = paymentId;
        UserId = userId;
        Amount = amount;
        Currency = currency;
        OrderId = orderId;
        Source = "PaymentGateway";
    }

    public Guid PaymentId { get; }
    public Guid UserId { get; }
    public decimal Amount { get; }
    public CurrencyCode Currency { get; }
    public string? OrderId { get; }
}

public record PaymentVerifiedEvent : IntegrationEvent
{
    public PaymentVerifiedEvent(Guid paymentId, Guid userId, decimal amount, CurrencyCode currency, string referenceId, string? orderId)
    {
        PaymentId = paymentId;
        UserId = userId;
        Amount = amount;
        Currency = currency;
        ReferenceId = referenceId;
        OrderId = orderId;
        Source = "PaymentGateway";
    }

    public Guid PaymentId { get; }
    public Guid UserId { get; }
    public decimal Amount { get; }
    public CurrencyCode Currency { get; }
    public string ReferenceId { get; }
    public string? OrderId { get; }
}

public record PaymentFailedEvent : IntegrationEvent
{
    public PaymentFailedEvent(Guid paymentId, Guid userId, string errorMessage, string? orderId)
    {
        PaymentId = paymentId;
        UserId = userId;
        ErrorMessage = errorMessage;
        OrderId = orderId;
        Source = "PaymentGateway";
    }

    public Guid PaymentId { get; }
    public Guid UserId { get; }
    public string ErrorMessage { get; }
    public string? OrderId { get; }
}