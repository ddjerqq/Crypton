namespace Crypton.Domain.Common.Extensions;

public static class DateTimeExtensions
{
    public static DateTime ToDateTime(this long timestampMs)
    {
        var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return origin.AddMilliseconds(timestampMs);
    }

    public static long ToUnixTimestampMs(this DateTime date)
    {
        var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        var diff = date.ToUniversalTime() - origin;
        return (long)Math.Floor(diff.TotalMilliseconds);
    }
}