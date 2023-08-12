using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Crypton.Infrastructure.Persistence.ValueConverters;

public sealed class DateTimeUtcValueConverter : ValueConverter<DateTime, DateTime>
{
    public DateTimeUtcValueConverter()
        : base(
            v => v,
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}