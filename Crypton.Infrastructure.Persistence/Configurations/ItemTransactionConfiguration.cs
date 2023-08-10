using Crypton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crypton.Infrastructure.Persistence.Configurations;

public class ItemTransactionConfiguration : IEntityTypeConfiguration<ItemTransaction>
{
    public void Configure(EntityTypeBuilder<ItemTransaction> builder)
    {
        builder.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId);
    }
}