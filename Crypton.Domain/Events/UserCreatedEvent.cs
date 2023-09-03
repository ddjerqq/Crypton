using Crypton.Domain.Common.Abstractions;

namespace Crypton.Domain.Events;

public sealed record UserCreatedEvent(Guid UserId) : IDomainEvent;