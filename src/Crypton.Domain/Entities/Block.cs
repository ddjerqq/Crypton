using System.Security.Cryptography;

namespace Crypton.Domain.Entities;

public sealed record Block(
    int Difficulty,
    long Index,
    long TimestampMs,
    byte[] PreviousHash)
{
    public const int PayloadSize = sizeof(long) + sizeof(long) + 32 + 32 + sizeof(long);

    private readonly List<Transaction> _transactions = new();

    public IEnumerable<Transaction> Transactions => _transactions;

    public double TotalFee => Transactions.Sum(x => x.Fee);

    public long Nonce { get; set; }

    public byte[] Payload
    {
        get
        {
            var buffer = new byte[PayloadSize];
            var offset = 0;

            Buffer.BlockCopy(BitConverter.GetBytes(Index), 0, buffer, offset, sizeof(long));
            offset += sizeof(long);

            Buffer.BlockCopy(BitConverter.GetBytes(TimestampMs), 0, buffer, offset, sizeof(long));
            offset += sizeof(long);

            Buffer.BlockCopy(PreviousHash, 0, buffer, offset, 32);
            offset += 32;

            Buffer.BlockCopy(MerkleRoot, 0, buffer, offset, 32);
            offset += 32;

            Buffer.BlockCopy(BitConverter.GetBytes(Nonce), 0, buffer, offset, sizeof(long));

            return buffer;
        }
    }

    // ReSharper disable once ReturnTypeCanBeEnumerable.Global
    public byte[] Hash => SHA256.HashData(Payload);

    // check that the hash starts with the required number of zeroes
    public bool IsHashValid => Hash.Take(Difficulty).All(x => x == 0);

    public byte[] MerkleRoot => Common.MerkleRoot.BuildMerkleRoot(Transactions.Select(x => x.Hash).ToList());

    public void AddTransaction(Transaction transaction) => _transactions.Add(transaction);

    public void Mine()
    {
        int coreCount = Environment.ProcessorCount;

        Parallel.For(0, coreCount, (i, loopState) =>
        {
            using var sha256 = SHA256.Create();
            long nonce = i;
            var payload = Payload;

            while (!loopState.IsStopped)
            {
                var nonceBytes = BitConverter.GetBytes(nonce);
                Buffer.BlockCopy(nonceBytes, 0, payload, PayloadSize - sizeof(long), sizeof(long));

                byte[] hash = sha256.ComputeHash(payload);

                if (hash[..Difficulty].All(x => x == 0))
                {
                    Nonce = nonce;
                    loopState.Stop();
                }

                nonce += coreCount;
            }
        });
    }
}