using Crypton.Domain.Entities;
using Crypton.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crypton.Infrastructure.Persistence.EntityTypeConfigurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(e => e.UserName)
            .IsUnique();

        builder.Ignore(x => x.DomainEvents);

        builder.Property(x => x.Wallet)
            .HasConversion(
                wallet => wallet.Balance,
                balance => new Wallet(balance));

        builder.HasMany(e => e.Inventory)
            .WithOne(x => x.Owner)
            .HasForeignKey(x => x.OwnerId);

        builder.OwnsOne<DailyStreak>(x => x.DailyStreak, dailyStreak =>
        {
            dailyStreak.Property<Guid>("id");
            dailyStreak.HasKey("id");

            dailyStreak.Property(x => x.DailyCollectedAt)
                .HasColumnName(nameof(DailyStreak.DailyCollectedAt))
                .HasDefaultValue(new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            dailyStreak.Property(x => x.Streak)
                .HasColumnName(nameof(DailyStreak.Streak))
                .HasDefaultValue(0);
        });
    }
}