namespace Crypton.Domain.ValueObjects;

public sealed record Rarity(float Value)
{
    public float Value { get; } = Value;

    public static Rarity CreateForItemType(ItemType itemType) =>
        new(RandBetween(itemType.MinRarity, itemType.MaxRarity));

    private static float RandBetween(float min, float max) =>
        ((float)Random.Shared.NextDouble() * (max - min)) + min;
}