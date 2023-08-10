using System.Text.Json;
using Crypton.Domain.Common.Extensions;

namespace Crypton.Infrastructure.Policies;

public sealed class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) => name.ToSnakeCase();
}