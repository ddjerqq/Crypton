namespace Crypton.Domain.Common.Abstractions;

public interface IAggregateRoot : IEntity
{
}

public abstract class AggregateRootBase : EntityBase, IAggregateRoot
{
}