using Crypton.Domain.Entities;
using Crypton.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Crypton.Application.Common.Interfaces;

public interface IAppDbContext : IDisposable
{
    public DbSet<T> Set<T>()
        where T : class;

    public EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
        where TEntity : class;

    public Task<int> SaveChangesAsync(CancellationToken ct = default);
}