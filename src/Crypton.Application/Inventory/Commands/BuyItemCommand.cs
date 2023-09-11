using Crypton.Domain.Entities;
using ErrorOr;
using MediatR;

namespace Crypton.Application.Inventory.Commands;

public sealed record BuyItemCommand(string ItemTypeId) : IRequest<ErrorOr<Item>>;