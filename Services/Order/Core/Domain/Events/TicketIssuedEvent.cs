using BuildingBlocks.Messaging.Events;

namespace Order.Domain.Events;

public record TicketIssuedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public long OrderItemId { get; init; }
    public string TicketNumber { get; init; }
    public string PNR { get; init; }

    public TicketIssuedEvent(Guid orderId, long orderItemId, string ticketNumber, string pnr)
    {
        OrderId = orderId;
        OrderItemId = orderItemId;
        TicketNumber = ticketNumber;
        PNR = pnr;
        Source = "OrderService";
    }
}