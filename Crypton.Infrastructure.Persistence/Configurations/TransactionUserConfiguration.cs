using Crypton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crypton.Infrastructure.Persistence.Configurations;

public class TransactionUserConfiguration : IEntityTypeConfiguration<TransactionUser>
{
    public void Configure(EntityTypeBuilder<TransactionUser> builder)
    {
        // this is for the linker table
        builder.HasKey(cs => new { cs.TransactionId, cs.UserId, cs.IsSender });

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);

        builder.HasOne(x => x.Transaction)
            .WithMany(s => s.Participants)
            .HasForeignKey(x => x.TransactionId);
    }
}