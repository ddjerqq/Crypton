// <copyright file="AppDbContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

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
    public static readonly AuditableEntitySaveChangesInterceptor AuditableEntitySaveChangesInterceptor = new();
    public static readonly UserMaterializationInterceptor UserMaterializationInterceptor = new();

    private readonly IMediator mediator;

    public AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator)
        : base(options)
    {
        this.mediator = mediator;
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
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var entityTableName = entity.GetTableName()!
                .Replace("AspNet", string.Empty)
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