using BuildingBlocks.CQRS;
using WalletPayment.Domain.Common.Contracts;
using WalletPayment.Domain.Exceptions;

namespace WalletPayment.Application.Features.Wallets.GetWalletSummary;

/// <summary>
/// Get wallet summary handler
/// </summary>
public class GetWalletSummaryHandler : IQueryHandler<GetWalletSummaryQuery, WalletSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetWalletSummaryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<WalletSummaryDto> Handle(GetWalletSummaryQuery request, CancellationToken cancellationToken)
    {
        // Get wallet with all related data
        var wallet = await _unitOfWork.Wallets.GetByUserIdWithIncludesAsync(
            request.UserId,
            includeCurrencyAccounts: true,
            includeBankAccounts: true,
            cancellationToken: cancellationToken);

        if (wallet == null)
        {
            throw new WalletNotFoundException(request.UserId);
        }

        // Get recent transactions (last 10)
        var (recentTransactions, _) = await _unitOfWork.Transactions.GetUserTransactionsAsync(
            request.UserId,
            page: 1,
            pageSize: 10,
            cancellationToken: cancellationToken);

        // Get transaction statistics
        var statistics = await _unitOfWork.Transactions.GetTransactionStatisticsAsync(
            request.UserId,
            cancellationToken: cancellationToken);

        // Get current month statistics
        var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var currentMonthStats = await _unitOfWork.Transactions.GetTransactionStatisticsAsync(
            request.UserId,
            fromDate: currentMonthStart,
            toDate: DateTime.UtcNow,
            cancellationToken: cancellationToken);

        // Get total balance in IRR
        var totalBalanceInIrr = await _unitOfWork.Wallets.GetTotalBalanceInIrrAsync(wallet.Id, cancellationToken);

        return new WalletSummaryDto
        {
            WalletId = wallet.Id,
            UserId = wallet.UserId,
            IsActive = wallet.IsActive,
            TotalBalanceInIrr = totalBalanceInIrr,
            CurrencyBalances = wallet.CurrencyAccounts
                .Where(a => a.IsActive && !a.IsDeleted)
                .Select(a => new CurrencyBalanceDto
                {
                    Currency = a.Currency,
                    Balance = a.Balance.Value,
                    IsActive = a.IsActive
                }),
            RecentTransactions = recentTransactions.Select(t => new RecentTransactionDto
            {
                Id = t.Id,
                TransactionNumber = t.TransactionNumber.Value,
                Amount = t.Amount.Value,
                Currency = t.Amount.Currency,
                Direction = t.Direction,
                Type = t.Type,
                Status = t.Status,
                Description = t.Description,
                TransactionDate = t.TransactionDate
            }),
            Statistics = new WalletStatisticsDto
            {
                TotalTransactions = statistics.TotalTransactions,
                SuccessfulTransactions = statistics.SuccessfulTransactions,
                TotalDeposits = statistics.TotalDeposits,
                TotalWithdrawals = statistics.TotalWithdrawals,
                CurrentMonthTransactions = currentMonthStats.TotalTransactions
            },
            BankAccounts = wallet.BankAccounts
                .Where(ba => !ba.IsDeleted)
                .Select(ba => new BankAccountSummaryDto
                {
                    Id = ba.Id,
                    BankName = ba.BankName,
                    MaskedAccountNumber = ba.GetMaskedAccountNumber(),
                    IsDefault = ba.IsDefault,
                    IsVerified = ba.IsVerified
                })
        };
    }
}