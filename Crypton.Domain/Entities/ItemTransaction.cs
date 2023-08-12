﻿namespace Crypton.Domain.Entities;

public class ItemTransaction : Transaction
{
    public Guid ItemId { get; init; }

    public Item Item { get; set; } = null!;

    protected override string Payload => string.Format(base.Payload, this.ItemId);
}