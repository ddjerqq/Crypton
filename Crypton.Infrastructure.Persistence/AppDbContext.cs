// <copyright file="AppDbContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Crypton.Application.Common.Interfaces;
using Crypton.Domain.Common.Extensions;
using Crypton.Domain.Entities;
using Crypton.Infrastructure.Persistence.Common.Extensions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Crypton.Infrastructure.Persistence;

public sealed class AppDbContext : IdentityDbContext<User>, IAppDbContext
{
    private readonly IMediator mediator;

    public AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator)
        : base(options)
    {
        this.mediator = mediator;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        await mediator.DispatchDomainEvents(this);
        return await base.SaveChangesAsync(ct);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entity in builder.Model.GetEntityTypes())
        {
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

        builder.Entity<User>().ToTable("user");
        builder.Entity<IdentityRole>().ToTable("role");
        builder.Entity<IdentityUserRole<string>>().ToTable("user_role");
        builder.Entity<IdentityUserClaim<string>>().ToTable("user_claim");
        builder.Entity<IdentityUserLogin<string>>().ToTable("user_login");
        builder.Entity<IdentityUserToken<string>>().ToTable("user_token");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("role_claim");
    }
}