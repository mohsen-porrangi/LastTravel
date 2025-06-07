using BuildingBlocks.Data;
using Microsoft.EntityFrameworkCore;
using WalletPayment.Domain.Aggregates.TransactionAggregate;
using WalletPayment.Domain.Common.Contracts;
using WalletPayment.Domain.Enums;
using WalletPayment.Domain.TransactionAggregate;
using WalletPayment.Domain.ValueObjects;
using WalletPayment.Infrastructure.Persistence.Context;

namespace WalletPayment.Infrastructure.Persistence.Repositories;

/// <summary>
/// Transaction repository implementation
/// </summary>
public class TransactionRepository : RepositoryBase<Transaction, Guid, WalletDbContext>, ITransactionRepository
{
    public TransactionRepository(WalletDbContext context) : base(context)
    {
    }

    public async Task<Transaction?> GetByTransactionNumberAsync(
        TransactionNumber transactionNumber,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.Wallet)
            .Include(t => t.CurrencyAccount)
            .FirstOrDefaultAsync(t =>
                t.TransactionNumber.Value == transactionNumber.Value &&
                !t.IsDeleted, cancellationToken);
    }

    public async Task<Transaction?> GetByPaymentReferenceAsync(
        string paymentReferenceId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.Wallet)
            .Include(t => t.CurrencyAccount)
            .FirstOrDefaultAsync(t =>
                t.PaymentReferenceId == paymentReferenceId &&
                !t.IsDeleted, cancellationToken);
    }

    public async Task<(IEnumerable<Transaction> Transactions, int TotalCount)> GetUserTransactionsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        TransactionType? type = null,
        TransactionDirection? direction = null,
        CurrencyCode? currency = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(t => t.CurrencyAccount)
            .Where(t => t.UserId == userId && !t.IsDeleted);

        // Apply filters
        if (type.HasValue)
            query = query.Where(t => t.Type == type.Value);

        if (direction.HasValue)
            query = query.Where(t => t.Direction == direction.Value);

        if (currency.HasValue)
            query = query.Where(t => t.Amount.Currency == currency.Value);

        if (fromDate.HasValue)
            query = query.Where(t => t.TransactionDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.TransactionDate <= toDate.Value);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (transactions, totalCount);
    }

    public async Task<IEnumerable<Transaction>> GetAccountTransactionsAsync(
        Guid accountId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Where(t => t.CurrencyAccountId == accountId && !t.IsDeleted);

        if (fromDate.HasValue)
            query = query.Where(t => t.TransactionDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.TransactionDate <= toDate.Value);

        query = query.OrderByDescending(t => t.TransactionDate);

        if (limit.HasValue)
            query = query.Take(limit.Value);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetRefundableTransactionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        return await DbSet
            .Include(t => t.CurrencyAccount)
            .Where(t => t.UserId == userId &&
                       t.Status == TransactionStatus.Completed &&
                       t.Direction == TransactionDirection.Out &&
                       t.Type != TransactionType.Refund &&
                       t.ProcessedAt.HasValue &&
                       t.ProcessedAt.Value > thirtyDaysAgo &&
                       !t.IsDeleted)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetRelatedTransactionsAsync(
        Guid originalTransactionId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.RelatedTransactionId == originalTransactionId && !t.IsDeleted)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetPendingTransactionsOlderThanAsync(
        int minutes,
        CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-minutes);

        return await DbSet
            .Where(t => t.Status == TransactionStatus.Pending &&
                       t.TransactionDate < cutoffTime &&
                       !t.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> PaymentReferenceExistsAsync(
        string paymentReferenceId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(t => t.PaymentReferenceId == paymentReferenceId && !t.IsDeleted, cancellationToken);
    }

    public async Task<TransactionStatistics> GetTransactionStatisticsAsync(
        Guid? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(t => !t.IsDeleted);

        if (userId.HasValue)
            query = query.Where(t => t.UserId == userId.Value);

        if (fromDate.HasValue)
            query = query.Where(t => t.TransactionDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.TransactionDate <= toDate.Value);

        var stats = await query
            .GroupBy(t => 1)
            .Select(g => new
            {
                TotalTransactions = g.Count(),
                SuccessfulTransactions = g.Count(t => t.Status == TransactionStatus.Completed),
                FailedTransactions = g.Count(t => t.Status == TransactionStatus.Failed),
                TotalAmount = g.Where(t => t.Status == TransactionStatus.Completed).Sum(t => t.Amount.Value),
                TotalDeposits = g.Where(t => t.Status == TransactionStatus.Completed && t.Direction == TransactionDirection.In).Sum(t => t.Amount.Value),
                TotalWithdrawals = g.Where(t => t.Status == TransactionStatus.Completed && t.Direction == TransactionDirection.Out).Sum(t => t.Amount.Value)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new TransactionStatistics
        {
            TotalTransactions = stats?.TotalTransactions ?? 0,
            SuccessfulTransactions = stats?.SuccessfulTransactions ?? 0,
            FailedTransactions = stats?.FailedTransactions ?? 0,
            TotalAmount = stats?.TotalAmount ?? 0,
            TotalDeposits = stats?.TotalDeposits ?? 0,
            TotalWithdrawals = stats?.TotalWithdrawals ?? 0,
            Currency = CurrencyCode.IRR,
            FromDate = fromDate ?? DateTime.MinValue,
            ToDate = toDate ?? DateTime.MaxValue
        };
    }

    public async Task AddSnapshotAsync(TransactionSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        await Context.TransactionSnapshots.AddAsync(snapshot, cancellationToken);
    }

    public async Task<IEnumerable<TransactionSnapshot>> GetAccountSnapshotsAsync(
        Guid accountId,
        SnapshotType? type = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.TransactionSnapshots
            .Where(s => s.AccountId == accountId);

        if (type.HasValue)
            query = query.Where(s => s.Type == type.Value);

        if (fromDate.HasValue)
            query = query.Where(s => s.SnapshotDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(s => s.SnapshotDate <= toDate.Value);

        return await query
            .OrderByDescending(s => s.SnapshotDate)
            .ToListAsync(cancellationToken);
    }
}