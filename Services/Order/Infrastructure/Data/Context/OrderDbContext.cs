using BuildingBlocks.Contracts;
using BuildingBlocks.Extensions;
using BuildingBlocks.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Order.Application.Common.Interfaces;
using Order.Domain.Entities;

namespace Order.Infrastructure.Data.Context;

public class OrderDbContext : DbContext, IOrderDbContext
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<OrderDbContext> _logger;
    private IDbContextTransaction? _currentTransaction;

    public OrderDbContext(DbContextOptions<OrderDbContext> options, IMessageBus messageBus, ILogger<OrderDbContext> logger)
        : base(options)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    public DbSet<Domain.Entities.Order> Orders => Set<Domain.Entities.Order>();
    public DbSet<OrderFlight> OrderFlights => Set<OrderFlight>();
    public DbSet<OrderTrain> OrderTrains => Set<OrderTrain>();
    public DbSet<OrderTrainCarTransport> OrderTrainCarTransports => Set<OrderTrainCarTransport>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<OrderWalletTransaction> OrderWalletTransactions => Set<OrderWalletTransaction>();
    public DbSet<SavedPassenger> SavedPassengers => Set<SavedPassenger>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ChangeTracker.SetAuditProperties();

        var entities = ChangeTracker.Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        //  اضافه کردن error handling برای Domain Events
        await PublishDomainEventsAsync(entities, cancellationToken);

        return result;
    }

    //  جداسازی منطق publishing events با error handling
    private async Task PublishDomainEventsAsync(List<IHasDomainEvents> entities, CancellationToken cancellationToken)
    {
        foreach (var entity in entities)
        {
            var events = entity.DomainEvents.ToList();
            entity.ClearDomainEvents();

            foreach (var @event in events)
            {
                try
                {
                    await _messageBus.PublishAsync(@event, cancellationToken);
                    _logger.LogDebug("Successfully published domain event {EventType} for entity {EntityType}",
                        @event.GetType().Name, entity.GetType().Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish domain event {EventType} for entity {EntityType}",
                        @event.GetType().Name, entity.GetType().Name);

                    // در صورت خطا، event را دوباره به entity اضافه می‌کنیم برای retry بعدی
                    entity.AddDomainEvent(@event);

                    // یا می‌توانیم exception را رethrow کنیم اگر می‌خواهیم transaction fail شود
                    // throw;
                }
            }
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (_currentTransaction != null) return;
        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        if (_currentTransaction == null) return;

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

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
    {
        if (_currentTransaction == null) return;

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
}