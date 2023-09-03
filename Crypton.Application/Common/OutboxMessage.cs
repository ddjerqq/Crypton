using Crypton.Domain.Common.Abstractions;
using Newtonsoft.Json;

namespace Crypton.Application.Common;

public sealed class OutboxMessage
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
        {
            NamingStrategy = new Newtonsoft.Json.Serialization.SnakeCaseNamingStrategy(),
        },
    };

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Type { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime OccuredOnUtc { get; set; }

    public DateTime? ProcessedOnUtc { get; set; }

    public string? Error { get; set; }

    public static OutboxMessage FromDomainEvent(IDomainEvent domainEvent)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = domainEvent.GetType().Name,
            Content = JsonConvert.SerializeObject(domainEvent, JsonSerializerSettings),
            OccuredOnUtc = DateTime.UtcNow,
            ProcessedOnUtc = null,
            Error = null,
        };
    }
}