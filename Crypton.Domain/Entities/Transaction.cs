// <copyright file="Transaction.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations.Schema;
using Crypton.Domain.Common.Abstractions;
using Crypton.Domain.Common.Extensions;

namespace Crypton.Domain.Entities;

public class Transaction : BaseDomainEntity
{
    public static readonly int Difficulty;
    protected static readonly string Predicate;

    static Transaction()
    {
        Difficulty = int.Parse(Environment.GetEnvironmentVariable("BLOCKCHAIN_DIFFICULTY") ?? "4");
        Predicate = new string('0', Difficulty);
    }

    public Guid Id { get; init; }

    public long Index { get; init; }

    public ICollection<TransactionUser> Participants { get; init; } = null!;

    [NotMapped]
    public User Sender
    {
        get => this.Participants.First(x => x.IsSender).User;
        init
        {
            var sender = this.Participants.First(x => x.IsSender);
            sender.User = value;

            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            sender.UserId = value?.Id ?? "00000000-0000-0000-0000-000000000000";
        }
    }

    [NotMapped]
    public User Receiver
    {
        get => this.Participants.First(x => x.IsReceiver).User;
        init
        {
            var receiver =
                this.Participants.First(x => x.IsReceiver);
            receiver.User = value;

            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            receiver.UserId = value?.Id ?? "00000000-0000-0000-0000-000000000000";
        }
    }

    public DateTime Timestamp { get; init; }

    public long Nonce { get; protected set; }

    public string PreviousHash { get; init; } = string.Empty;

    public string Hash => this.Payload.CalculateSha256HexDigest();

    public bool IsValid => this.Hash[Difficulty..] == Predicate;

    protected virtual string Payload =>
        $"{this.Id}{this.Index}{this.Sender.Id}{this.Receiver.Id}" +
        $"{{0}}{this.Timestamp.Ticks}{this.Nonce}{this.PreviousHash}";

    public static Transaction Genesis()
    {
        var zeroGuid = Guid.Parse("00000000-0000-0000-0000-000000000000");

        var block = new Transaction
        {
            Id = zeroGuid,
            Index = 0,
            Sender = null!,
            Receiver = null!,
            Timestamp = new DateTime(2015, 1, 1),
            Nonce = 0,
            PreviousHash = new string('0', 64),
        };

        block.Mine()
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

        return block;
    }

    public Task Mine(CancellationToken ct = default)
    {
        return Task.Run(() =>
        {
            while (!ct.IsCancellationRequested || this.IsValid)
            {
                this.Nonce++;
            }
        }, ct);
    }

    public Transaction Next(User sender, User receiver, decimal amount)
    {
        return new BalanceTransaction
        {
            Id = Guid.NewGuid(),
            Index = this.Index + 1,
            Sender = sender,
            Receiver = receiver,
            Timestamp = DateTime.UtcNow,
            Nonce = 0,
            PreviousHash = this.PreviousHash,

            Amount = amount,
        };
    }

    public Transaction Next(User sender, User receiver, Item item)
    {
        return new ItemTransaction
        {
            Id = Guid.NewGuid(),
            Index = this.Index + 1,
            Sender = sender,
            Receiver = receiver,
            Timestamp = DateTime.UtcNow,
            Nonce = 0,
            PreviousHash = this.PreviousHash,

            ItemId = item.Id,
            Item = item,
        };
    }
}