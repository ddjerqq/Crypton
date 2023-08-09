// <copyright file="AuditableEntitySaveChangesInterceptor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Crypton.Domain.Common.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Crypton.Infrastructure.Persistence.Interceptors;

public sealed class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    public static void UpdateEntities(DbContext? context)
    {
        // have some kind of user manager get the user id who performed the action here
        context?.ChangeTracker
            .Entries<IAuditableDomainEntity>()
            .ToList()
            .ForEach(entry =>
            {
                if (entry.State == EntityState.Added)
                    entry.Entity.Created = DateTime.UtcNow;

                if (entry.State == EntityState.Added || entry.State == EntityState.Modified ||
                    HasChangedOwnedEntities(entry))
                    entry.Entity.LastModified = DateTime.UtcNow;
            });
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        UpdateEntities(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, ct);
    }

    private static bool HasChangedOwnedEntities(EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            r.TargetEntry.State is EntityState.Added or EntityState.Modified);
}