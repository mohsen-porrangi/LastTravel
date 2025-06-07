namespace UserManagement.API.Infrastructure.Data;

using BuildingBlocks.Contracts;
using BuildingBlocks.Messaging.Contracts;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.API.Common;

public class UnitOfWork(UserDbContext db, IMessageBus messageBus) : IUnitOfWork
{
    private IDbContextTransaction _currentTransaction;
    private bool _disposed = false;

    public IUserRepository Users { get; } = new UserRepository(db);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // ذخیره‌سازی تغییرات
        var result = await db.SaveChangesAsync(cancellationToken);

        // انتشار رویدادهای دامنه
        await PublishDomainEventsAsync(cancellationToken);

        return result;
    }

    private async Task PublishDomainEventsAsync(CancellationToken cancellationToken)
    {
        // یافتن تمام موجودیت‌هایی که رویدادهای دامنه دارند
        var entities = db.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Any())
            .ToList();

        // جمع‌آوری همه رویدادها
        var domainEvents = entities
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        // پاک‌سازی رویدادها از موجودیت‌ها
        entities.ForEach(e => e.Entity.ClearDomainEvents());

        // انتشار هر رویداد در باس پیام
        foreach (var domainEvent in domainEvents)
        {
            await messageBus.PublishAsync(domainEvent, cancellationToken);
        }
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            return _currentTransaction;
        }

        _currentTransaction = await db.Database.BeginTransactionAsync(cancellationToken);
        return _currentTransaction;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);

            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _currentTransaction?.Dispose();
            db.Dispose();
        }
        _disposed = true;
    }
}