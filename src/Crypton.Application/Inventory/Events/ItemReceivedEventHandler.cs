using Crypton.Domain.Events;
using MediatR;

namespace Crypton.Application.Inventory.Events;

internal sealed class ItemReceivedEventHandler : INotificationHandler<ItemReceivedEvent>
{
    public Task Handle(ItemReceivedEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Email sending is not implemented.");
    }
}