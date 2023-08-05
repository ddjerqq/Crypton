// <copyright file="UserConfiguration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Crypton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crypton.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("user");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserName)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(e => e.Balance)
            .IsRequired();

        builder.Property(e => e.Created)
            .HasDefaultValueSql("now()");

        builder.HasIndex(e => e.UserName)
            .IsUnique();
    }
}