using BuildingBlocks.CQRS;
using Order.Application.Common.DTOs;

namespace Order.Application.Passengers.Queries.GetSavedPassengers;

public record GetSavedPassengersQuery : IQuery<List<SavedPassengerDto>>;