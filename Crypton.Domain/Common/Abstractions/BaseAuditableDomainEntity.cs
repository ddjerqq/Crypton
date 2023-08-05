// <copyright file="BaseAuditableDomainEntity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations.Schema;
using MediatR;

namespace Crypton.Domain.Common.Abstractions;

public abstract class BaseAuditableDomainEntity : IAuditableDomainEntity
{
    [NotMapped]
    public ICollection<INotification> ProtectedDomainEvents { get; set; } = new List<INotification>();

    public DateTime Created { get; set; }

    public string? CratedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public string? LastModifiedBy { get; set; }
}