using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MediatR;

namespace Crypton.Domain.Common.Abstractions;

public interface IEntity
{
    [JsonIgnore]
    protected ICollection<INotification> ProtectedDomainEvents { get; set; }

    [JsonIgnore]
    public IEnumerable<INotification> DomainEvents => this.ProtectedDomainEvents;

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

public abstract class EntityBase
{
    [NotMapped]
    [JsonIgnore]
    public ICollection<INotification> ProtectedDomainEvents { get; set; } = new List<INotification>();
}