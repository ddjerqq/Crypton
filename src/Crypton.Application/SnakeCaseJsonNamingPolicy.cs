﻿using System.Text.Json;
using Crypton.Domain.Common.Extensions;

namespace Crypton.Application;

public sealed class SnakeCaseJsonNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) => name.ToSnakeCase();
}