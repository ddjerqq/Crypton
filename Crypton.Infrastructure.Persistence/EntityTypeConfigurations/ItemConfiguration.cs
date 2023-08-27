using Crypton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crypton.Infrastructure.Persistence.EntityTypeConfigurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasOne(e => e.Owner)
            .WithMany(x => x.Inventory) // inventory is List<Item>
            .HasForeignKey(x => x.OwnerId);

        builder.HasOne(e => e.ItemType)
            .WithMany()
            .HasForeignKey(x => x.ItemTypeId);
    }
}