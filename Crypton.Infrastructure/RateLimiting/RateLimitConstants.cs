using Microsoft.Extensions.Configuration;

namespace Crypton.Infrastructure.RateLimiting;

public static class RateLimitConstants
{
    public const string TransactionPolicyName = "transaction";
    public const string GlobalPolicyName = "global";

    public static IEnumerable<RateLimitOptions> LoadRateLimitOptions(IConfiguration configuration)
    {
        var options = new List<RateLimitOptions>();

        configuration.GetSection("RateLimitPolicies")
            .Bind(options);

        return options;
    }
}