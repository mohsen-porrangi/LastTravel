namespace Order.Domain.Exceptions;

public class TicketingException : OrderDomainException
{
    public string? PNR { get; }
    public string? Provider { get; }

    public TicketingException(string message, string? pnr = null, string? provider = null)
        : base(message)
    {
        PNR = pnr;
        Provider = provider;
    }
}