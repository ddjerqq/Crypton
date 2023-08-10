using Crypton.Domain.Entities;
using Crypton.Domain.ValueTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Crypton.Application.Interfaces;

public interface IAppDbContext : IDisposable
{
    public DbSet<User> Users { get; }

    public DbSet<Item> Items { get; }

    public DbSet<Transaction> Transactions { get; }

    public DbSet<TransactionUser> TransactionUsers { get; }

    public DbSet<ItemType> ItemTypes { get; }

    public EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
        where TEntity : class;

    public Task<int> SaveChangesAsync(CancellationToken ct = default);
}