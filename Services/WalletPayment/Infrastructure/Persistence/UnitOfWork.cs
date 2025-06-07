using Microsoft.EntityFrameworkCore.Storage;
using WalletPayment.Domain.Common.Contracts;
using WalletPayment.Infrastructure.Persistence.Context;
using WalletPayment.Infrastructure.Persistence.Repositories;

namespace WalletPayment.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation for wallet domain
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly WalletDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    // Lazy-loaded repositories
    private IWalletRepository? _wallets;
    private ITransactionRepository? _transactions;

    public UnitOfWork(WalletDbContext context)
    {
        _context = context;
    }

    public IWalletRepository Wallets =>
        _wallets ??= new WalletRepository(_context);

    public ITransactionRepository Transactions =>
        _transactions ??= new TransactionRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
            throw new InvalidOperationException("Transaction already in progress");

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return _currentTransaction;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
            throw new InvalidOperationException("No transaction in progress");

        try
        {
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
            throw new InvalidOperationException("No transaction in progress");

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            // Already in transaction, just execute
            return await operation(cancellationToken);
        }

        using var transaction = await BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await operation(cancellationToken);
            await CommitTransactionAsync(cancellationToken);
            return result;
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        await ExecuteInTransactionAsync(async ct =>
        {
            await operation(ct);
            return true;
        }, cancellationToken);
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
    }
}