using Microsoft.Extensions.Logging;
using Order.Domain.Enums;

namespace Order.Infrastructure.ExternalServices.Flight;

public class FlightServiceClient(ILogger<FlightServiceClient> logger) : IFlightService
{
    public Task<FlightSearchResponse> SearchFlightsAsync(FlightSearchRequest request, CancellationToken cancellationToken)
    {
        // Mock implementation
        logger.LogInformation("Searching flights from {Source} to {Destination}",
            request.SourceCode, request.DestinationCode);

        var flights = new List<FlightOption>
        {
            new("IR301", FlightProvider.IranAir,
                request.DepartureDate.AddHours(8),
                request.DepartureDate.AddHours(9).AddMinutes(30),
                2_500_000m, 50),

            new("W5081", FlightProvider.Mahan,
                request.DepartureDate.AddHours(14),
                request.DepartureDate.AddHours(15).AddMinutes(30),
                2_200_000m, 30)
        };

        return Task.FromResult(new FlightSearchResponse(flights));
    }

    public Task<FlightReserveResponse> ReserveFlightAsync(FlightReserveRequest request, CancellationToken cancellationToken)
    {
        // Mock implementation
        logger.LogInformation("Reserving flight {FlightNumber} for {PassengerCount} passengers",
            request.FlightNumber, request.Passengers.Count);

        var pnr = GeneratePNR();
        return Task.FromResult(new FlightReserveResponse(true, pnr, DateTime.UtcNow.AddMinutes(30)));
    }

    public Task<FlightTicketResponse> IssueTicketAsync(FlightTicketRequest request, CancellationToken cancellationToken)
    {
        // Mock implementation
        logger.LogInformation("Issuing tickets for PNR {PNR}", request.PNR);

        var tickets = new List<TicketInfo>
        {
            new($"TKT{Random.Shared.Next(100000, 999999)}", "Test Passenger", $"{Random.Shared.Next(1, 30)}{(char)('A' + Random.Shared.Next(0, 6))}")
        };

        return Task.FromResult(new FlightTicketResponse(true, tickets));
    }

    private static string GeneratePNR()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
    }
}