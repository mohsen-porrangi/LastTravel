using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using WalletPayment.Domain.TransactionAggregate;
using WalletPayment.Domain.Aggregates.WalletAggregate;
using WalletPayment.Domain.Aggregates.TransactionAggregate;

namespace WalletPayment.Application.Common.Interfaces;

/// <summary>
/// Database context interface for wallet domain
/// </summary>
public interface IWalletDbContext
{
    /// <summary>
    /// Wallets DbSet
    /// </summary>
    DbSet<Wallet> Wallets { get; }

    /// <summary>
    /// Currency Accounts DbSet
    /// </summary>
    DbSet<CurrencyAccount> CurrencyAccounts { get; }

    /// <summary>
    /// Bank Accounts DbSet
    /// </summary>
    DbSet<BankAccount> BankAccounts { get; }

    /// <summary>
    /// Credits DbSet (B2B)
    /// </summary>
    DbSet<Credit> Credits { get; }

    /// <summary>
    /// Transactions DbSet
    /// </summary>
    DbSet<Transaction> Transactions { get; }

    /// <summary>
    /// Transaction Snapshots DbSet
    /// </summary>
    DbSet<TransactionSnapshot> TransactionSnapshots { get; }

    /// <summary>
    /// Database facade for transactions
    /// </summary>
    DatabaseFacade Database { get; }

    /// <summary>
    /// Save changes asynchronously
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Save changes synchronously
    /// </summary>
    int SaveChanges();
}