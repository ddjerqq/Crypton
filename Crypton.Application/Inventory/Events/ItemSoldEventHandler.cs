using Crypton.Domain.Events;
using MediatR;

namespace Crypton.Application.Inventory.Events;

internal sealed class ItemSoldEventHandler : INotificationHandler<ItemReceivedEvent>
{
    public async Task Handle(ItemReceivedEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}