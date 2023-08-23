using Crypton.Domain.Common.Abstractions;

namespace Crypton.Domain.ValueObjects;

public sealed record ItemType(string Id, string Name, decimal Price, float MinRarity, float MaxRarity)
    : ValueObjectBase;