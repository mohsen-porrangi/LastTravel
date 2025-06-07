using BuildingBlocks.CQRS;
using MediatR;
using Microsoft.Extensions.Logging;
using Order.Application.Common.Interfaces;
using Order.Domain.Contracts;

namespace Order.Application.Common.Behaviors;

public class TransactionBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    IOrderDbContext dbContext,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        try
        {
            logger.LogDebug("Beginning transaction for {RequestName}", requestName);

            await unitOfWork.BeginTransactionAsync(cancellationToken);

            var response = await next();

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogDebug("Committed transaction for {RequestName}", requestName);

            return response;
        }
        catch
        {
            logger.LogError("Rolling back transaction for {RequestName}", requestName);

            await unitOfWork.RollbackTransactionAsync(cancellationToken);

            throw;
        }
    }
}
//TODO check for need move to buling block or not?