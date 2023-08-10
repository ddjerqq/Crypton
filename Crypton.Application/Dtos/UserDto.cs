using Crypton.Domain.Entities;

namespace Crypton.Application.Dtos;

public sealed class UserDto
{
    public Guid Id { get; set; }

    public string? UserName { get; set; } = string.Empty;

    public string? Email { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public DateTime Created { get; set; }

    public IEnumerable<ItemDto> Items { get; set; } = new List<ItemDto>();

    public static implicit operator UserDto(User user)
    {
        return new UserDto
        {
            Id = Guid.Parse(user.Id),
            UserName = user.UserName,
            Email = user.Email,
            Balance = user.Balance,
            Created = user.Created,
            Items = user.Items.Select(item => (ItemDto)item),
        };
    }
}