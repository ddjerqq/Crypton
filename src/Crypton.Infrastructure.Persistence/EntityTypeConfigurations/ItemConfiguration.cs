using Crypton.Domain.Entities;
using Crypton.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crypton.Infrastructure.Persistence.EntityTypeConfigurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.OwnsOne<Rarity>(x => x.Rarity, rarity =>
        {
            rarity.Property<Guid>("id");
            rarity.HasKey("id");

            rarity.Property(x => x.Value)
                .HasColumnName(nameof(Rarity))
                .IsRequired();
        });

        builder.HasOne(e => e.Owner)
            .WithMany(x => x.Inventory) // inventory is List<Item>
            .HasForeignKey(x => x.OwnerId);

        builder.HasOne(e => e.ItemType)
            .WithMany()
            .HasForeignKey(x => x.ItemTypeId);
    }
}