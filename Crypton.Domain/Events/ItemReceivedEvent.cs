using Crypton.Domain.Common.Abstractions;

namespace Crypton.Domain.Events;

public sealed record ItemReceivedEvent(Guid ItemId) : IDomainEvent;