using System.ComponentModel.DataAnnotations.Schema;
using Crypton.Domain.Common.Extensions;

namespace Crypton.Domain.Entities;

public sealed class TransactionUser
{
    public TransactionUser(string userId, Guid transactionId, bool isSender)
    {
        this.TransactionId = transactionId;
        this.UserId = userId;
        this.IsSender = isSender;
    }

    public TransactionUser(User user, Guid transactionId, bool isSender)
        : this(user.Id, transactionId, isSender)
    {
        this.User = user;
    }

    public TransactionUser(User user, Transaction transaction, bool isSender)
        : this(user, transaction.Id, isSender)
    {
        this.Transaction = transaction;
    }

    public Guid TransactionId { get; init; }

    public Transaction Transaction { get; init; } = null!;

    public string UserId { get; set; }

    public User User { get; set; } = null!;

    public bool IsSender { get; init; }

    [NotMapped]
    public bool IsReceiver => !this.IsSender;

    [NotMapped]
    public bool IsSystem => this.UserId == GuidExtensions.ZeroGuidValue;
}