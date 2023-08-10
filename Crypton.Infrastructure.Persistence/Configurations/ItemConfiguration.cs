using Crypton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crypton.Infrastructure.Persistence.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasOne(e => e.Owner)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.OwnerId);

        builder.HasOne(e => e.ItemType)
            .WithMany()
            .HasForeignKey(e => e.ItemTypeId);
    }
}