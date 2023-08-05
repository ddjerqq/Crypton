// <copyright file="IAuditableDomainEntity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Crypton.Domain.Common.Abstractions;

public interface IAuditableDomainEntity : IDomainEntity
{
    public DateTime Created { get; set; }

    public string? CratedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public string? LastModifiedBy { get; set; }
}