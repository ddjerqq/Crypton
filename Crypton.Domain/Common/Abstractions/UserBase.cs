using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Crypton.Domain.Common.Abstractions;

public class UserBase : IdentityUser<Guid>, IAuditableEntity
{
    [JsonIgnore]
    public ICollection<INotification> ProtectedDomainEvents { get; set; } = new List<INotification>();


    public DateTime? Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? Deleted { get; set; }

    public string? DeletedBy { get; set; }
}