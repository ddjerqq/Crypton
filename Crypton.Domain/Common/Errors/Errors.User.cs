using ErrorOr;

namespace Crypton.Domain.Common.Errors;

public static partial class Errors
{
    public static class User
    {
        public static Error NotFound = Error.Failure("user.not_found", "the user is not found");
        public static Error Unauthenticated = Error.Failure("user.unauthenticated", "the user is not authenticated");

        public static Error DailyNotReady(DateTime collectNextAt) => Error.Failure(
            "user.daily_not_ready",
            $"you are not elligible to collect the daily reward yet, you can collect it at {collectNextAt:F}");
    }
}