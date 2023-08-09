// <copyright file="TransactionUser.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations.Schema;

namespace Crypton.Domain.Entities;

public sealed class TransactionUser
{
    public Guid TransactionId { get; init; }

    public Transaction Transaction { get; init; } = null!;

    public string UserId { get; set; } = string.Empty;

    public User User { get; set; } = null!;

    public bool IsSender { get; init; }

    [NotMapped]
    public bool IsReceiver => !this.IsSender;
}