using Crypton.Domain.Entities;

namespace Crypton.Application.Dto;

public sealed class UserDto
{
    public Guid Id { get; set; }

    public string? UserName { get; set; }

    public string? Email { get; set; }

    public decimal Balance { get; set; }

    public DateTime? Created { get; set; }

    public IEnumerable<ItemDto> Items { get; set; } = new List<ItemDto>();

    public static implicit operator UserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Balance = user.Wallet.Balance,
            Created = user.Created,
            Items = user.Inventory.Select(item => (ItemDto)item).ToList(),
        };
    }
}