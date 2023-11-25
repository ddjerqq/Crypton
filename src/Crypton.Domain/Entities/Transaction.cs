using System.Security.Cryptography;
using Crypton.Domain.Common.Extensions;
using Crypton.Domain.ValueObjects;

namespace Crypton.Domain.Entities;

public sealed record Transaction(Wallet Sender, Wallet Receiver, double Amount, double Fee, long TimestampMs)
{
    public const int PayloadSize = 42 + 42 + sizeof(double) + sizeof(double) + sizeof(long);

    public DateTime Timestamp => DateTimeOffset.FromUnixTimeMilliseconds(TimestampMs).UtcDateTime;

    public byte[] Payload
    {
        get
        {
            var buffer = new byte[PayloadSize];
            var offset = 0;

            Buffer.BlockCopy(Sender.Address.ToHexBytes(), 0, buffer, offset, 42);
            offset += 42;

            Buffer.BlockCopy(Receiver.Address.ToHexBytes(), 0, buffer, offset, 42);
            offset += 42;

            Buffer.BlockCopy(BitConverter.GetBytes(Amount), 0, buffer, offset, sizeof(double));
            offset += sizeof(double);

            Buffer.BlockCopy(BitConverter.GetBytes(Fee), 0, buffer, offset, sizeof(double));
            offset += sizeof(double);

            Buffer.BlockCopy(BitConverter.GetBytes(TimestampMs), 0, buffer, offset, sizeof(long));

            return buffer;
        }
    }

    public byte[] Hash => SHA256.HashData(Payload);

    public byte[] Signature => Sender.Sign(Payload);
}