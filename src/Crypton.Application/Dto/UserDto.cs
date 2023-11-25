using Crypton.Domain.Entities;
using Crypton.Domain.ValueObjects;

namespace Crypton.Application.Dto;

public sealed class UserDto
{
    public Guid Id { get; init; }

    public string? UserName { get; init; }

    public string? Email { get; init; }

    public DateTime? Created { get; init; }

    public string? CreatedBy { get; init; } = string.Empty;

    public DateTime? LastModified { get; init; }

    public string? LastModifiedBy { get; init; } = string.Empty;

    public decimal Balance { get; init; }

    public IEnumerable<ItemDto> Items { get; init; } = new List<ItemDto>();

    public DailyStreak DailyStreak { get; init; } = null!;

    public static implicit operator UserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Balance = 0,
            // TODO: bring back balance Balance = user.Wallet.Balance,
            Created = user.Created,
            CreatedBy = user.CreatedBy,
            LastModified = user.LastModified,
            LastModifiedBy = user.LastModifiedBy,
            Items = user.Inventory.Select(item => (ItemDto)item).ToList(),
            DailyStreak = user.DailyStreak,
        };
    }
}