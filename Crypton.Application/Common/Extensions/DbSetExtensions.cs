using Crypton.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Crypton.Application.Common.Extensions;

public static class DbSetExtensions
{
    public static EntityEntry<TEntity>? TryUpdateIfNotNull<TEntity>(this DbSet<TEntity> dbSet, TEntity? entity)
        where TEntity : class
    {
        try
        {
            if (entity is not null)
            {
                return dbSet.Update(entity);
            }
        }
        catch (InvalidOperationException ex)
        {
            // the entity is already being tracked by the DbContext
            // common causes: the entity is the current user.
            var logger = dbSet.GetService<ILogger<IAppDbContext>>();
            logger.LogDebug(ex, "The entity is already being tracked by the DbContext");
            logger.LogInformation("The entity is already being tracked by the DbContext");
        }

        return null;
    }
}