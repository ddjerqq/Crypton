using Crypton.Domain.Entities;
using Crypton.Domain.ValueObjects;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Crypton.Application.Inventory.Commands;

public sealed record ItemTransactionCommand(User? Sender, User? Receiver, Item Item) : IRequest<IErrorOr>
{
    public IInventory SenderInventory { get; } = (IInventory?)Sender?.Inventory ?? new MagicInventory();

    public IInventory ReceiverInventory { get; } = (IInventory?)Receiver?.Inventory ?? new MagicInventory();
}

/// <summary>
/// This is a magic inventory that always has the item
/// It is used when the sender is null, and the receiver is the current user
/// This is used in the <see cref="BuyItemCommand"/>
/// </summary>
internal sealed class MagicInventory : List<Item>, IInventory
{
    public bool HasItemWithId(Guid _) => true;

    public void Transfer(IInventory other, Item item) => other.Add(item);
}

internal sealed class ItemTransactionValidator : AbstractValidator<ItemTransactionCommand>
{
    public ItemTransactionValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Sender)
            .NotEmpty()
            .When(x => x.Receiver is null)
            .WithMessage("Either the sender or the receiver must be present");

        RuleFor(x => x.Receiver)
            .NotEmpty()
            .When(x => x.Sender is null)
            .WithMessage("Either the sender or the receiver must be present");

        RuleFor(x => x.Item)
            .NotEmpty();
    }
}