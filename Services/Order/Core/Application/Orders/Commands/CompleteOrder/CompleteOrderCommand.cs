using BuildingBlocks.CQRS;

namespace Order.Application.Orders.Commands.CompleteOrder;

public record CompleteOrderCommand(Guid OrderId) : ICommand;