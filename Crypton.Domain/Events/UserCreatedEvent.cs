using Crypton.Domain.Common.Abstractions;
using Crypton.Domain.Common.Errors;

namespace Crypton.Domain.Events;

public sealed record UserCreatedEvent(Guid UserId) : IDomainEvent;