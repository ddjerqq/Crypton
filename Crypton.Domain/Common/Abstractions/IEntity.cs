using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MediatR;

namespace Crypton.Domain.Common.Abstractions;

public interface IEntity
{
    [NotMapped]
    [JsonIgnore]
    public IEnumerable<INotification> DomainEvents { get; }

    public void AddDomainEvent(INotification domainEvent);

    public void RemoveDomainEvent(INotification domainEvent);

    public void ClearDomainEvents();
}

public abstract class EntityBase
{
    public IEnumerable<INotification> DomainEvents => this.ProtectedDomainEvents;

    [NotMapped]
    [JsonIgnore]
    protected ICollection<INotification> ProtectedDomainEvents { get; } = new List<INotification>();

    public void AddDomainEvent(INotification domainEvent)
    {
        this.ProtectedDomainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(INotification domainEvent)
    {
        this.ProtectedDomainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        this.ProtectedDomainEvents.Clear();
    }
}