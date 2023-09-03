using System.Text.Json.Serialization;
using Crypton.Domain.ValueObjects;
using MediatR;

namespace Crypton.Application.Inventory;

public abstract record BuyItemResult
{
    public sealed record Success : BuyItemResult;

    public sealed record InsufficientFunds : BuyItemResult;
}

public sealed class BuyItemCommand : IRequest<BuyItemResult>
{
    [JsonRequired]
    public ItemType ItemType { get; set; } = null!;
}

// public sealed class
