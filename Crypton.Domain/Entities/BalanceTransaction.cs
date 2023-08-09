// <copyright file="BalanceTransaction.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Crypton.Domain.Entities;

public sealed class BalanceTransaction : Transaction
{
    public decimal Amount { get; init; }

    protected override string Payload => string.Format(base.Payload, this.Amount);
}