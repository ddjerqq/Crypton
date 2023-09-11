using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace Crypton.Domain.Common.Abstractions;

public class UserBase : IdentityUser<Guid>, IAggregateRoot, IAuditableEntity
{
    [NotMapped]
    [JsonIgnore]
    private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();

    public DateTime? Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? Deleted { get; set; }

    public string? DeletedBy { get; set; }

    public IEnumerable<IDomainEvent> DomainEvents => _domainEvents;

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}