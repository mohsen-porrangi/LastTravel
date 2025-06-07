using BuildingBlocks.CQRS;
using WalletPayment.Domain.Common.Contracts;

namespace WalletPayment.Application.Features.Transactions.GetTransactionHistory;

/// <summary>
/// Get transaction history handler
/// </summary>
public class GetTransactionHistoryHandler : IQueryHandler<GetTransactionHistoryQuery, GetTransactionHistoryResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTransactionHistoryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetTransactionHistoryResult> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
    {
        var (transactions, totalCount) = await _unitOfWork.Transactions.GetUserTransactionsAsync(
            request.UserId,
            request.Page,
            request.PageSize,
            request.Type,
            request.Direction,
            request.Currency,
            request.FromDate,
            request.ToDate,
            cancellationToken);

        var transactionDtos = transactions.Select(t => new TransactionDto
        {
            Id = t.Id,
            TransactionNumber = t.TransactionNumber.Value,
            Amount = t.Amount.Value,
            Currency = t.Amount.Currency,
            Direction = t.Direction,
            Type = t.Type,
            Status = t.Status,
            Description = t.Description,
            TransactionDate = t.TransactionDate,
            ProcessedAt = t.ProcessedAt,
            PaymentReferenceId = t.PaymentReferenceId,
            OrderContext = t.OrderContext
        });

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        var pagination = new PaginationInfo
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = request.Page < totalPages,
            HasPreviousPage = request.Page > 1
        };

        return new GetTransactionHistoryResult
        {
            Transactions = transactionDtos,
            Pagination = pagination
        };
    }
}