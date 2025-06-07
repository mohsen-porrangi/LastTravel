using BuildingBlocks.Messaging.Events;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuildingBlocks.Contracts;

/// <summary>
/// رابط برای موجودیت‌هایی که رویدادهای دامنه دارند
/// </summary>
public interface IHasDomainEvents
{
    [NotMapped]
    IReadOnlyCollection<IntegrationEvent> DomainEvents { get; }
    void AddDomainEvent(IntegrationEvent @event);
    void RemoveDomainEvent(IntegrationEvent @event);
    void ClearDomainEvents();
}