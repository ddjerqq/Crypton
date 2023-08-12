using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MediatR;

namespace Crypton.Domain.Common.Abstractions;

public class BaseDomainEntity : IDomainEntity
{
    [NotMapped]
    [JsonIgnore]
    public ICollection<INotification> ProtectedDomainEvents { get; set; } = new List<INotification>();
}