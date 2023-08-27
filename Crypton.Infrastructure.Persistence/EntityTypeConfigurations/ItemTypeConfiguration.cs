using Crypton.Domain.Entities;
using Crypton.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crypton.Infrastructure.Persistence.EntityTypeConfigurations;

public class ItemTypeConfiguration : IEntityTypeConfiguration<ItemType>
{
    public void Configure(EntityTypeBuilder<ItemType> builder)
    {
        builder.HasIndex(e => e.Name)
            .IsUnique();

        builder.HasMany<Item>()
            .WithOne(x => x.ItemType)
            .HasForeignKey(x => x.ItemTypeId);

        this.SeedData(builder);
    }

    private void SeedData(EntityTypeBuilder<ItemType> builder)
    {
        var data = new List<ItemType>
        {
            new ItemType("FISHING_ROD", "Fishing rod 🎣", 75m, 0.1f, 0.9f),
            new ItemType("HUNTING_RIFLE", "Hunting Rifle 🔫", 75m, 0.1f, 0.9f),
            new ItemType("SHOVEL", "Shovel 🪣", 75m, 0.1f, 0.9f),
            new ItemType("COMMON_FISH", "Common Fish 🐟", 5, 0.1f, 0.9f),
            new ItemType("RARE_FISH", "Rare Fish 🐡", 10, 0.1f, 0.9f),
            new ItemType("TROPICAL_FISH", "Tropical Fish 🐯", 20, 0.1f, 0.9f),
            new ItemType("SHARK", "Shark 🐠", 40, 0.1f, 0.9f),
            new ItemType("GOLDEN_FISH", "Golden Fish 🦈", 50, 0.1f, 0.9f),
            new ItemType("PIG", "Pig 🥇🐟", 5, 0.1f, 0.9f),
            new ItemType("DEER", "Deer 🐷", 10, 0.1f, 0.9f),
            new ItemType("BEAR", "Bear 🦌", 20, 0.1f, 0.9f),
            new ItemType("WOLF", "Wolf 🐺", 30, 0.1f, 0.9f),
            new ItemType("TIGER", "Tiger 🐻", 40, 0.1f, 0.9f),
            new ItemType("LION", "Lion 🦁", 50, 0.1f, 0.9f),
            new ItemType("ELEPHANT", "Elephant 🐯", 60, 0.1f, 0.9f),
            new ItemType("COPPER_COIN", "Copper Coin 🐘", 1, 0.1f, 0.9f),
            new ItemType("EMERALD", "Emerald 👛", 10, 0.1f, 0.9f),
            new ItemType("RUBY", "Ruby 🔶", 20, 0.1f, 0.9f),
            new ItemType("SAPPHIRE", "Sapphire 🔷", 30, 0.1f, 0.9f),
            new ItemType("AMETHYST", "Amethyst 🔴", 40, 0.1f, 0.9f),
            new ItemType("DIAMOND", "Diamond 💎", 50, 0.1f, 0.9f),
            new ItemType("KNIFE", "Knife 🔪", 50, 0.1f, 0.9f),
            new ItemType("WEDDING_RING", "Wedding Ring 💍", 1000, 0.1f, 0.9f),
        };

        builder.HasData(data);
    }
}