using Crypton.Application.Interfaces;
using Crypton.Domain.Common.Extensions;
using Crypton.Domain.Entities;
using Crypton.Domain.ValueObjects;
using Crypton.Infrastructure.Persistence.Common.Extensions;
using Crypton.Infrastructure.Persistence.Interceptors;
using Crypton.Infrastructure.Persistence.ValueConverters;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Crypton.Infrastructure.Persistence;

public sealed class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IAppDbContext
{
    private readonly IMediator _mediator;
    private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IMediator mediator,
        AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor)
        : base(options)
    {
        this._mediator = mediator;
        this._auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
    }

    public DbSet<Item> Items => this.Set<Item>();

    public DbSet<ItemType> ItemTypes => this.Set<ItemType>();

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        await this._mediator.DispatchDomainEvents(this);
        return await base.SaveChangesAsync(ct);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(builder);

        SnakeCaseRename(builder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(this._auditableEntitySaveChangesInterceptor);
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

    private static void SeedData(ModelBuilder builder)
    {
        builder.Entity<User>()
            .HasData(new List<User>
            { });

        builder.Entity<ItemType>()
            .HasData(new List<ItemType>
            { });

        builder.Entity<Item>()
            .HasData(new List<Item>
            { });
    }
}