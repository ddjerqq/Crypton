using Crypton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crypton.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(e => e.UserName)
            .IsUnique();

        builder.HasMany(e => e.Items)
            .WithOne(x => x.Owner)
            .HasForeignKey(x => x.OwnerId);
    }
}