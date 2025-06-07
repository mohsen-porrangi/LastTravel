using BuildingBlocks.Messaging.Events;

namespace Order.Application.IntegrationEvents.Events;

public record TicketIssuedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string TicketNumber { get; init; }
    public string PassengerName { get; init; }
    public string Mobile { get; init; }

    public TicketIssuedIntegrationEvent(Guid orderId, string ticketNumber, string passengerName, string mobile)
    {
        OrderId = orderId;
        TicketNumber = ticketNumber;
        PassengerName = passengerName;
        Mobile = mobile;
        Source = "OrderService";
    }
}