// <copyright file="IRules.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Crypton.Diamond;

public interface IRules
{
    public Guid Salt { get; init; }
    public int HashHead { get; init; }
    public int HashTail { get; init; }
    public byte ChecksumStart { get; init; }
    public int[] ChecksumIndexes { get; init; }
    public UInt128 AppToken { get; init; }
    public string AppTokenDigest { get; }
    public string HashFormat { get; }

    public string? Sign(string? uri, string? userId, DateTime? timestamp);
}