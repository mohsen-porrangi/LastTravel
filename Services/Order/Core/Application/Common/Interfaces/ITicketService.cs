namespace Order.Application.Common.Interfaces
{
    public interface ITicketService
    {
        Task<byte[]> GenerateTicketPdfAsync(long orderItemId, CancellationToken cancellationToken = default);
        string GenerateBarcode(string ticketNumber);
    }
}
