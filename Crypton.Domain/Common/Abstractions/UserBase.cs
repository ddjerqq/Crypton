using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Crypton.Domain.Common.Abstractions;

public class UserBase : IdentityUser<Guid>, IAuditableEntity
{
    public DateTime? Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? Deleted { get; set; }

    public string? DeletedBy { get; set; }

    [NotMapped]
    [JsonIgnore]
    public IEnumerable<INotification> DomainEvents => this.ProtectedDomainEvents;

    [NotMapped]
    [JsonIgnore]
    protected ICollection<INotification> ProtectedDomainEvents { get; set; } = new List<INotification>();

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