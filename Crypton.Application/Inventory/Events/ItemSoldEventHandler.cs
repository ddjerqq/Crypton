using Crypton.Domain.Events;
using MediatR;

namespace Crypton.Application.Inventory.Events;

internal sealed class ItemSoldEventHandler : INotificationHandler<ItemReceivedEvent>
{
    public Task Handle(ItemReceivedEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}