using Crypton.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crypton.Infrastructure.Persistence.EntityTypeConfigurations;

public class ItemTypeConfiguration : IEntityTypeConfiguration<ItemType>
{
    public void Configure(EntityTypeBuilder<ItemType> builder)
    {
        builder.HasIndex(e => e.Id)
            .IsUnique();

        builder.HasIndex(e => e.Name)
            .IsUnique();
    }
}