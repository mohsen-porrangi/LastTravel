using Microsoft.Extensions.Logging;
using Order.Application.Common.DTOs;
using Order.Infrastructure.ExternalServices.Common;

namespace Order.Infrastructure.ExternalServices.Train
{
    public class TrainServiceClient(
      HttpClient httpClient,
      ILogger<TrainServiceClient> logger)
      : BaseHttpClient(httpClient, logger), ITrainService
    {
        // Similar mock implementation as FlightServiceClient
        public Task<TrainSearchResult> SearchTrainsAsync(TrainSearchRequest request, CancellationToken cancellationToken = default)
        {
            var mockResult = new TrainSearchResult
            {
                Trains = new List<TrainInfo>
            {
                new()
                {
                    TrainNumber = "R145",
                    Provider = "Raja",
                    DepartureTime = DateTime.Now.AddDays(1),
                    ArrivalTime = DateTime.Now.AddDays(1).AddHours(5),
                    BasePrice = 500000
                }
            }
            };

            return Task.FromResult(mockResult);
        }

        public Task<TrainReserveResult> ReserveTrainAsync(TrainReserveRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new TrainReserveResult
            {
                Success = true,
                PNR = $"TPNR{DateTime.Now.Ticks}"
            });
        }

        public Task<TrainTicketResult> IssueTicketAsync(TrainTicketRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new TrainTicketResult
            {
                Success = true,
                TicketNumber = $"TTKT{DateTime.Now.Ticks}",
                PdfUrl = "https://mock-url.com/train-ticket.pdf"
            });
        }
    }
}
