using Crypton.Application.Common;
using Crypton.Domain.Common.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Crypton.Infrastructure.Persistence.Interceptors;

public sealed class ConvertDomainEventsToOutboxMessagesInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        DbContext? dbContext = eventData.Context;

        if (dbContext is null)
            return await base.SavingChangesAsync(eventData, result, ct);

        var outboxMessages = dbContext.ChangeTracker
            .Entries<IAggregateRoot>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                var domainEvents = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();
                return domainEvents;
            })
            .Select(OutboxMessage.FromDomainEvent)
            .ToList();

        await dbContext.Set<OutboxMessage>()
            .AddRangeAsync(outboxMessages, ct);

        return await base.SavingChangesAsync(eventData, result, ct);
    }
}