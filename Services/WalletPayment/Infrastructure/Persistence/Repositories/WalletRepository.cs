using BuildingBlocks.Data;
using Microsoft.EntityFrameworkCore;
using WalletPayment.Domain.Aggregates.WalletAggregate;
using WalletPayment.Domain.Common.Contracts;
using WalletPayment.Domain.Enums;
using WalletPayment.Infrastructure.Persistence.Context;

namespace WalletPayment.Infrastructure.Persistence.Repositories;

/// <summary>
/// Wallet repository implementation
/// </summary>
public class WalletRepository : RepositoryBase<Wallet, Guid, WalletDbContext>, IWalletRepository
{
    public WalletRepository(WalletDbContext context) : base(context)
    {
    }

    public async Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(w => w.CurrencyAccounts)
            .FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted, cancellationToken);
    }

    public async Task<Wallet?> GetByUserIdWithIncludesAsync(
        Guid userId,
        bool includeCurrencyAccounts = true,
        bool includeBankAccounts = false,
        bool includeCredits = false,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (includeCurrencyAccounts)
        {
            query = query.Include(w => w.CurrencyAccounts);
        }

        if (includeBankAccounts)
        {
            query = query.Include(w => w.BankAccounts);
        }

        if (includeCredits)
        {
            query = query.Include(w => w.Credits);
        }

        return await query
            .FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted, cancellationToken);
    }

    public async Task<bool> UserHasWalletAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(w => w.UserId == userId && !w.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<Wallet>> GetWalletsWithCreditsDueSoonAsync(
        int daysFromNow = 7,
        CancellationToken cancellationToken = default)
    {
        var dueDate = DateTime.UtcNow.AddDays(daysFromNow);

        return await DbSet
            .Include(w => w.Credits)
            .Where(w => w.IsActive && !w.IsDeleted)
            .Where(w => w.Credits.Any(c =>
                c.Status == CreditStatus.Active &&
                c.DueDate <= dueDate &&
                !c.IsDeleted))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Wallet>> GetWalletsWithOverdueCreditsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await DbSet
            .Include(w => w.Credits)
            .Where(w => w.IsActive && !w.IsDeleted)
            .Where(w => w.Credits.Any(c =>
                c.Status == CreditStatus.Active &&
                c.DueDate < now &&
                !c.IsDeleted))
            .ToListAsync(cancellationToken);
    }

    public async Task<CurrencyAccount?> GetCurrencyAccountByIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await Context.CurrencyAccounts
            .Include(ca => ca.Wallet)
            .FirstOrDefaultAsync(ca => ca.Id == accountId && !ca.IsDeleted, cancellationToken);
    }

    public async Task<CurrencyAccount?> GetCurrencyAccountAsync(
        Guid walletId,
        CurrencyCode currency,
        CancellationToken cancellationToken = default)
    {
        return await Context.CurrencyAccounts
            .FirstOrDefaultAsync(ca =>
                ca.WalletId == walletId &&
                ca.Currency == currency &&
                !ca.IsDeleted, cancellationToken);
    }

    public async Task<decimal> GetTotalBalanceInIrrAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        // For now, only IRR is supported
        // TODO: Add currency conversion when exchange service is ready
        var irrBalance = await Context.CurrencyAccounts
            .Where(ca => ca.WalletId == walletId &&
                        ca.Currency == CurrencyCode.IRR &&
                        ca.IsActive &&
                        !ca.IsDeleted)
            .SumAsync(ca => ca.Balance.Value, cancellationToken);

        return irrBalance;
    }

    public async Task<decimal> GetDailyTransactionAmountAsync(
        Guid walletId,
        CurrencyCode currency,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await Context.Transactions
            .Where(t => t.WalletId == walletId &&
                       t.Amount.Currency == currency &&
                       t.TransactionDate >= startOfDay &&
                       t.TransactionDate < endOfDay &&
                       t.Status == TransactionStatus.Completed &&
                       !t.IsDeleted)
            .SumAsync(t => t.Amount.Value, cancellationToken);
    }
}