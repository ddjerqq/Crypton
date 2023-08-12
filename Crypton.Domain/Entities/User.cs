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

    public bool IsSystem => this.Id == GuidExtensions.ZeroGuid;

    public static User SystemUser()
    {
        return new User
        {
            Id = GuidExtensions.ZeroGuid,
            UserName = "system",
            NormalizedUserName = "SYSTEM",
            Email = "system@crypton.com",
            NormalizedEmail = "SYSTEM@CRYPTON.COM",
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