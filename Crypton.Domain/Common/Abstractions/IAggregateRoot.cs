using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Crypton.Domain.Common.Abstractions;

public interface IAggregateRoot : IEntity
{
    [NotMapped]
    [JsonIgnore]
    public IEnumerable<IDomainEvent> DomainEvents { get; }

    public void AddDomainEvent(IDomainEvent domainEvent);

    public void ClearDomainEvents();
}

public abstract class AggregateRootBase : EntityBase, IAggregateRoot
{
    [NotMapped]
    [JsonIgnore]
    private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();

    public IEnumerable<IDomainEvent> DomainEvents => this._domainEvents;

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        this._domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        this._domainEvents.Clear();
    }
}