using Crypton.Domain.Common.Abstractions;
using Crypton.Domain.Entities;

namespace Crypton.Domain.Events;

public sealed record ItemSoldEvent(Item Item) : IDomainEvent;