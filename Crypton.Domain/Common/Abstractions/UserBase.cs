using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Crypton.Domain.Common.Abstractions;

public abstract class UserBase : IdentityUser<Guid>, IAuditableDomainEntity
{
    [NotMapped]
    [JsonIgnore]
    public ICollection<INotification> ProtectedDomainEvents { get; set; } = new List<INotification>();

    public DateTime Created { get; set; } = DateTime.UtcNow;

    public string? CreatedBy { get; set; }

    public DateTime? LastModified { get; set; } = DateTime.UtcNow;

    public string? LastModifiedBy { get; set; }
}