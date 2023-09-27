using System.Security.Claims;

namespace Crypton.Infrastructure.Diamond;

/// <summary>
/// this class represents the payload of each digitally signed http request.
/// </summary>
public sealed class RulePayload
{
    public RulePayload(string url, string appToken, string userId, string timestamp, string signature, ClaimsPrincipal user)
    {
        Url = url;
        AppToken = appToken;
        UserId = userId;
        Timestamp = timestamp;
        Signature = signature;
        User = user;
    }

    public string Url { get; set; }

    public string AppToken { get; set; }

    public string UserId { get; set; }

    public string Timestamp { get; set; }

    public string Signature { get; set; }

    public ClaimsPrincipal User { get; set; }
}