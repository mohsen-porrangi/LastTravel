using MediatR;
using Microsoft.Extensions.Logging;
using WalletPayment.Domain.Common.Contracts;

namespace WalletPayment.Application.Common.Behaviors;

/// <summary>
/// Transaction behavior for commands that need database transactions
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IUnitOfWork unitOfWork,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Only wrap commands that modify data in transactions
        if (!ShouldUseTransaction(requestName))
        {
            return await next();
        }

        _logger.LogInformation("Beginning transaction for {RequestName}", requestName);

        try
        {
            var response = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                return await next();
            }, cancellationToken);

            _logger.LogInformation("Transaction completed successfully for {RequestName}", requestName);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction failed for {RequestName}", requestName);
            throw;
        }
    }

    private static bool ShouldUseTransaction(string requestName)
    {
        // Commands that modify data should use transactions
        return requestName.Contains("Command") &&
               !requestName.Contains("Query") &&
               !requestName.Contains("Get");
    }
}