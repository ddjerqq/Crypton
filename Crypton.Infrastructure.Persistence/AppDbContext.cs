using Crypton.Application.Interfaces;
using Crypton.Domain.Common.Extensions;
using Crypton.Domain.Entities;
using Crypton.Domain.ValueTypes;
using Crypton.Infrastructure.Persistence.Common.Extensions;
using Crypton.Infrastructure.Persistence.Interceptors;
using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Crypton.Infrastructure.Persistence;

public sealed class AppDbContext : IdentityDbContext<User>, IAppDbContext
{
    private readonly AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor;
    private readonly UserMaterializationInterceptor userMaterializationInterceptor;

    private readonly IMediator mediator;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IMediator mediator,
        AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor,
        UserMaterializationInterceptor userMaterializationInterceptor)
        : base(options)
    {
        this.mediator = mediator;
        this.auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
        this.userMaterializationInterceptor = userMaterializationInterceptor;
    }

    public DbSet<Item> Items => this.Set<Item>();

    public DbSet<Transaction> Transactions => this.Set<Transaction>();

    public DbSet<TransactionUser> TransactionUsers => this.Set<TransactionUser>();

    public DbSet<ItemType> ItemTypes => this.Set<ItemType>();

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        await this.mediator.DispatchDomainEvents(this);
        return await base.SaveChangesAsync(ct);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<BalanceTransaction>();
        builder.Entity<ItemTransaction>();
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(builder);

        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var entityTableName = entity.GetTableName()!
                .Replace("AspNet", string.Empty)
                .TrimEnd('s')
                .ToSnakeCase();

            entity.SetTableName(entityTableName);

            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(property.GetColumnName().ToSnakeCase());
            }

            foreach (var key in entity.GetKeys())
            {
                key.SetName(key.GetName()!.ToSnakeCase());
            }

            foreach (var key in entity.GetForeignKeys())
            {
                key.SetConstraintName(key.GetConstraintName()!.ToSnakeCase());
            }

            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(index.GetDatabaseName()!.ToSnakeCase());
            }
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(
            this.auditableEntitySaveChangesInterceptor,
            this.userMaterializationInterceptor);

        base.OnConfiguring(optionsBuilder);
    }
}