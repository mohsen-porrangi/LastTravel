using BuildingBlocks.Contracts;
using BuildingBlocks.Messaging.Events;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuildingBlocks.Domain;

public abstract class EntityWithDomainEventsBase<TKey> : BaseEntity<TKey>, IHasDomainEvents
{
    private readonly List<IntegrationEvent> _domainEvents = new();
    [NotMapped]
    public IReadOnlyCollection<IntegrationEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IntegrationEvent @event) => _domainEvents.Add(@event);
    public void RemoveDomainEvent(IntegrationEvent @event) => _domainEvents.Remove(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

public abstract class EntityWithDomainEvents<TKey> : EntityWithDomainEventsBase<TKey>
{
}