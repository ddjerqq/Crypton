using Crypton.Domain.Common.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crypton.Infrastructure.Persistence.Common.Extensions;

public static class MediatorExtensions
{
    public static async Task DispatchDomainEvents(this IMediator mediator, DbContext context)
    {
        var entities = context
            .ChangeTracker
            .Entries<IEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        var futures = domainEvents.Select(ev => mediator.Publish(ev));
        await Task.WhenAll(futures);
    }
}