using ErrorOr;
using MediatR;

namespace Crypton.Application.Inventory.Commands;

public sealed record SendItemCommand(Guid ItemId, Guid ReceiverId) : IRequest<IErrorOr>;