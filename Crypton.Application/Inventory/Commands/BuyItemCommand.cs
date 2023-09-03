using Crypton.Application.Common.Interfaces;
using Crypton.Application.Economy.Commands;
using Crypton.Domain.Common.Errors;
using Crypton.Domain.Entities;
using Crypton.Domain.ValueObjects;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crypton.Application.Inventory.Commands;

public sealed class BuyItemCommand : IRequest<ErrorOr<Item>>
{
    public string ItemTypeId { get; set; } = string.Empty;
}

public sealed class BuyItemValidator : AbstractValidator<BuyItemCommand>
{
    public BuyItemValidator(IAppDbContext dbContext)
    {
        this.RuleFor(x => x.ItemTypeId)
            .Must(id => dbContext
                .Set<ItemType>()
                .Any(itemType => itemType.Id == id));
    }
}

public sealed class BuyItemHandler : IRequestHandler<BuyItemCommand, ErrorOr<Item>>
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public BuyItemHandler(IMediator mediator, IAppDbContext dbContext, ICurrentUserAccessor currentUserAccessor)
    {
        this._mediator = mediator;
        this._dbContext = dbContext;
        this._currentUserAccessor = currentUserAccessor;
    }

    public async Task<ErrorOr<Item>> Handle(BuyItemCommand request, CancellationToken ct)
    {
        var currentUser = await this._currentUserAccessor.GetCurrentUserAsync(ct);
        if (currentUser is null)
            return Errors.User.Unauthenticated;

        var itemType = await this._dbContext.Set<ItemType>()
            .FirstOrDefaultAsync(x => x.Id == request.ItemTypeId, ct);

        // pay for the item
        var payCommand = new CreateTransactionCommand(currentUser, null, itemType.Price);
        var payResult = await this._mediator.Send(payCommand, ct);
        if (payResult.IsError)
            return payResult.Errors!;

        // create the item and add it to the user's inventory
        var item = itemType.CreateItem(currentUser);
        var acquireItemCommand = new CreateTransactionCommand(null, currentUser, item);
        var acquireItemCommandResult = await this._mediator.Send(acquireItemCommand, ct);
        if (acquireItemCommandResult.IsError)
            return acquireItemCommandResult.Errors!;

        return item;
    }
}