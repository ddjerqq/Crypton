﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Crypton.Domain.Common.Abstractions;
using Crypton.Domain.Common.Extensions;
using dotenv.net;

namespace Crypton.Domain.Entities;

public class Transaction : BaseDomainEntity
{
    public static readonly int Difficulty;
    protected static readonly string Seed;
    protected static readonly string Predicate;

    static Transaction()
    {
        DotEnv.Fluent()
            .WithEnvFiles("./../.env")
            .Load();

        Difficulty = int.Parse(Environment.GetEnvironmentVariable("BLOCKCHAIN_DIFFICULTY") ?? "4");
        Predicate = new string('0', Difficulty);
        Seed = Environment.GetEnvironmentVariable("BLOCKCHAIN__SEED") ??
               throw new ArgumentException("BLOCKCHAIN__SEED is not present in the environment");
    }

    public Guid Id { get; init; }

    public long Index { get; init; }

    public ICollection<TransactionUser> Participants { get; set; } = new List<TransactionUser>(2);

    [NotMapped]
    public User Sender => this.Participants
        .FirstOrDefault(x => x.IsSender)?.User ?? User.SystemUser();

    [NotMapped]
    public User Receiver => this.Participants
        .FirstOrDefault(x => x.IsReceiver)?.User ?? User.SystemUser();

    public DateTime Timestamp { get; init; }

    public long Nonce { get; protected set; }

    public string PreviousHash { get; init; } = string.Empty;

    public string Hash => this.Payload.CalculateSha256HexDigest();

    public bool IsValid => this.Hash[..Difficulty] == Predicate;

    protected virtual string Payload =>
        $"{this.Id}{this.Index}" +
        $"{this.Sender.Id}{this.Receiver.Id}" +
        $"{{0}}" +
        $"|{this.Timestamp:s}|" +
        $"{this.Nonce}{this.PreviousHash}{Seed}";

    public static Transaction Genesis()
    {
        var transaction = new Transaction
        {
            Id = GuidExtensions.ZeroGuid,
            Index = 0,
            Participants = new TransactionUser[]
            {
                new(GuidExtensions.ZeroGuid, GuidExtensions.ZeroGuid, true),
                new(GuidExtensions.ZeroGuid, GuidExtensions.ZeroGuid, false),
            },
            Timestamp = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Nonce = 0,
            PreviousHash = new string('0', 64),
        };

        transaction.Mine()
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

        Debug.Assert(transaction.IsValid, "Genesis transaction is not valid");

        return transaction;
    }

    public Task Mine(CancellationToken ct = default)
    {
        return Task.Run(() =>
        {
            while (!ct.IsCancellationRequested)
            {
                if (this.IsValid) break;

                this.Nonce++;
            }
        }, ct);
    }

    public Transaction Next(User? sender, User? receiver, decimal amount)
    {
        var id = Guid.NewGuid();

        var participants = new TransactionUser[]
        {
            new(sender ?? User.SystemUser(), id, true),
            new(receiver ?? User.SystemUser(), id, false),
        };

        return new BalanceTransaction
        {
            Id = id,
            Index = this.Index + 1,
            Timestamp = DateTime.UtcNow,
            Nonce = 0,
            PreviousHash = this.Hash,
            Participants = participants,

            Amount = amount,
        };
    }

    public Transaction Next(User? sender, User? receiver, Item item)
    {
        var id = Guid.NewGuid();

        var participants = new TransactionUser[]
        {
            new(sender?.Id ?? GuidExtensions.ZeroGuid, id, true),
            new(receiver?.Id ?? GuidExtensions.ZeroGuid, id, false),
        };

        return new ItemTransaction
        {
            Id = id,
            Index = this.Index + 1,
            Timestamp = DateTime.UtcNow,
            Nonce = 0,
            PreviousHash = this.PreviousHash,
            Participants = participants,

            ItemId = item.Id,
            Item = item,
        };
    }

    public override string ToString()
    {
        return $"Transaction {{ " +
               $"Id={this.Id} Index={this.Index} Sender={this.Sender} " +
               $"Receiver={this.Receiver} Timestamp={this.Timestamp} Nonce={this.Nonce} " +
               $"PreviousHash={this.PreviousHash} " +
               $"Hash={this.Hash} }}";
    }
}