// <copyright file="BaseDomainEntity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations.Schema;
using MediatR;

namespace Crypton.Domain.Common.Abstractions;

public class BaseDomainEntity : IDomainEntity
{
    [NotMapped]
    public ICollection<INotification> ProtectedDomainEvents { get; set; } = new List<INotification>();
}