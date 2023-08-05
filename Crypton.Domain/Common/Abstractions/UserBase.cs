// <copyright file="UserBase.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations.Schema;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Crypton.Domain.Common.Abstractions;

public abstract class UserBase : IdentityUser, IAuditableDomainEntity
{
    [NotMapped]
    public ICollection<INotification> ProtectedDomainEvents { get; set; } = new List<INotification>();

    public DateTime Created { get; set; } = DateTime.UtcNow;

    public string? CratedBy { get; set; }

    public DateTime? LastModified { get; set; } = DateTime.UtcNow;

    public string? LastModifiedBy { get; set; }
}