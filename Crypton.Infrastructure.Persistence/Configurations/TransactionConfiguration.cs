﻿using Crypton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crypton.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasDiscriminator<string>("discriminator");

        builder.HasIndex(x => x.Index)
            .IsUnique();

        builder.HasMany(x => x.Participants)
            .WithOne(x => x.Transaction)
            .HasForeignKey(x => x.TransactionId);
    }
}