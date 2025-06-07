

using Order.Application.Common.DTOs;

namespace Order.Infrastructure.ExternalServices.Train;

public interface ITrainService
{
    Task<TrainSearchResult> SearchTrainsAsync(TrainSearchRequest request, CancellationToken cancellationToken = default);
    Task<TrainReserveResult> ReserveTrainAsync(TrainReserveRequest request, CancellationToken cancellationToken = default);
    Task<TrainTicketResult> IssueTicketAsync(TrainTicketRequest request, CancellationToken cancellationToken = default);
}

//public interface ITrainService
//{
//    Task<TrainSearchResponse> SearchTrainsAsync(TrainSearchRequest request, CancellationToken cancellationToken);
//    Task<TrainReserveResponse> ReserveTrainAsync(TrainReserveRequest request, CancellationToken cancellationToken);
//    Task<TrainTicketResponse> IssueTicketAsync(TrainTicketRequest request, CancellationToken cancellationToken);
//}

//public record TrainSearchRequest(
//    int SourceCode,
//    int DestinationCode,
//    DateTime DepartureDate,
//    int PassengerCount);

//public record TrainSearchResponse(
//    List<TrainOption> Trains);

//public record TrainOption(
//    string TrainNumber,
//    TrainProvider Provider,
//    DateTime DepartureTime,
//    DateTime ArrivalTime,
//    decimal BasePrice,
//    int AvailableSeats,
//    string WagonType);

//public record TrainReserveRequest(
//    string TrainNumber,