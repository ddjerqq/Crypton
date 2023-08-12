using Crypton.Domain.Entities;

namespace Crypton.Application.Dtos;

public class TransactionDto
{
    public Guid Id { get; init; }

    public long Index { get; init; }

    public Guid SenderId { get; init; }

    public string SenderUsername { get; init; } = string.Empty;

    public Guid ReceiverId { get; init; }

    public string ReceiverUserName { get; init; } = string.Empty;

    public ItemDto? Item { get; init; }

    public decimal? Amount { get; init; }

    public DateTime Timestamp { get; init; }

    public long Nonce { get; init; }

    public string PreviousHash { get; init; } = string.Empty;

    public string Payload { get; init; } = string.Empty;

    public string Hash { get; init; } = string.Empty;

    public static implicit operator TransactionDto(Transaction transaction)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            Index = transaction.Index,
            SenderId = Guid.Parse(transaction.Sender.Id),
            SenderUsername = transaction.Sender.UserName!,
            ReceiverId = Guid.Parse(transaction.Receiver.Id),
            ReceiverUserName = transaction.Receiver.UserName!,
            Item = transaction is ItemTransaction itemTransaction ? (ItemDto)itemTransaction.Item : null,
            Amount = transaction is BalanceTransaction balanceTransaction ? balanceTransaction.Amount : null,
            Timestamp = transaction.Timestamp,
            Nonce = transaction.Nonce,
            PreviousHash = transaction.PreviousHash,
            Hash = transaction.Hash,
            Payload = transaction.GetPayload(),
        };
    }
}