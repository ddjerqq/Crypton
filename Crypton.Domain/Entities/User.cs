using System.ComponentModel.DataAnnotations.Schema;
using Crypton.Domain.Common.Abstractions;
using Crypton.Domain.Common.Extensions;

namespace Crypton.Domain.Entities;

public sealed class User : UserBase
{
    [NotMapped]
    public decimal Balance { get; private set; }

    [NotMapped]
    public IReadOnlyCollection<Item> Items { get; private set; } = new List<Item>();

    public bool IsSystem => this.Id == GuidExtensions.ZeroGuidValue;

    public static User SystemUser()
    {
        return new User
        {
            Id = GuidExtensions.ZeroGuidValue,
            UserName = "System",
        };
    }

    public void SetBalance(decimal balance)
    {
        this.Balance = balance;
    }

    public void SetItems(IEnumerable<Item> items)
    {
        this.Items = items.ToList().AsReadOnly();
    }
}