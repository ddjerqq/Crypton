using Crypton.Application.Common.Interfaces;
using Crypton.Domain.ValueObjects;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Crypton.Application.Inventory.Commands;

public sealed record CreateItemTypeCommand(string Id, string Name, decimal Price, float MinRarity, float MaxRarity)
    : IRequest<ErrorOr<ItemType>>
{
    public ItemType ToItemType() => new(Id, Name, Price, MinRarity, MaxRarity);
}

public sealed class CreateItemTypeValidator : AbstractValidator<CreateItemTypeCommand>
{
    public CreateItemTypeValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .Matches(@"^[A-Z_]{3,50}$")
            .WithMessage("Id must be uppercase and contain only letters and underscores, 3-50 characters long");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.MinRarity)
            .GreaterThan(0)
            .LessThanOrEqualTo(x => x.MaxRarity);

        RuleFor(x => x.MaxRarity)
            .GreaterThanOrEqualTo(x => x.MinRarity)
            .LessThan(1);
    }
}

internal sealed class CreateItemTypeCommandHandler : IRequestHandler<CreateItemTypeCommand, ErrorOr<ItemType>>
{
    private readonly IAppDbContext _dbContext;

    public CreateItemTypeCommandHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<ItemType>> Handle(CreateItemTypeCommand request, CancellationToken ct)
    {
        var itemType = request.ToItemType();

        await _dbContext.Set<ItemType>().AddAsync(itemType, ct);
        await _dbContext.SaveChangesAsync(ct);

        return itemType;
    }
}