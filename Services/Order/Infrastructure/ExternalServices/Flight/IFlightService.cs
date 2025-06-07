using Order.Domain.Enums;

namespace Order.Infrastructure.ExternalServices.Flight;

public interface IFlightService
{
    Task<FlightSearchResponse> SearchFlightsAsync(FlightSearchRequest request, CancellationToken cancellationToken);
    Task<FlightReserveResponse> ReserveFlightAsync(FlightReserveRequest request, CancellationToken cancellationToken);
    Task<FlightTicketResponse> IssueTicketAsync(FlightTicketRequest request, CancellationToken cancellationToken);
}

public record FlightSearchRequest(
    int SourceCode,
    int DestinationCode,
    DateTime DepartureDate,
    int AdultCount,
    int ChildCount,
    int InfantCount);

public record FlightSearchResponse(
    List<FlightOption> Flights);

public record FlightOption(
    string FlightNumber,
    FlightProvider Provider,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    decimal BasePrice,
    int AvailableSeats);

public record FlightReserveRequest(
    string FlightNumber,
    List<PassengerInfo> Passengers);

public record PassengerInfo(
    string FirstName,
    string LastName,
    DateTime BirthDate,
    string NationalCode,
    string? PassportNumber);

public record FlightReserveResponse(
    bool Success,
    string PNR,
    DateTime ExpirationTime);

public record FlightTicketRequest(
    string PNR,
    string PaymentReference);

public record FlightTicketResponse(
    bool Success,
    List<TicketInfo> Tickets);

public record TicketInfo(
    string TicketNumber,
    string PassengerName,
    string SeatNumber);