using ErrorOr;
using MediatR;

namespace Crypton.Application.Inventory.Commands;

public sealed record SellItemCommand(Guid ItemId) : IRequest<ErrorOr<decimal>>;