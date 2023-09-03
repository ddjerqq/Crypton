using Crypton.Application.Common;
using Crypton.Application.Common.Interfaces;
using Crypton.Domain.Common.Extensions;
using Crypton.Domain.Entities;
using Crypton.Domain.ValueObjects;
using Crypton.Infrastructure.Persistence.Interceptors;
using Crypton.Infrastructure.Persistence.ValueConverters;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Crypton.Infrastructure.Persistence;

public sealed class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IAppDbContext
{
    private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;
    private readonly ConvertDomainEventsToOutboxMessagesInterceptor _convertDomainEventsToOutboxMessagesInterceptor;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor,
        ConvertDomainEventsToOutboxMessagesInterceptor convertDomainEventsToOutboxMessagesInterceptor)
        : base(options)
    {
        this._auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
        this._convertDomainEventsToOutboxMessagesInterceptor = convertDomainEventsToOutboxMessagesInterceptor;
    }

    public DbSet<Item> Items => this.Set<Item>();

    public DbSet<ItemType> ItemTypes => this.Set<ItemType>();

    public DbSet<OutboxMessage> OutboxMessages => this.Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(builder);

        SnakeCaseRename(builder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(this._auditableEntitySaveChangesInterceptor);
        optionsBuilder.AddInterceptors(this._convertDomainEventsToOutboxMessagesInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder
            .Properties<DateTime>()
            .HaveConversion<DateTimeUtcValueConverter>();

        base.ConfigureConventions(builder);
    }

    private static void SnakeCaseRename(ModelBuilder builder)
    {
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
}