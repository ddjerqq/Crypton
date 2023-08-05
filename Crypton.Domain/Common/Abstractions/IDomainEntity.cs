// <copyright file="IDomainEntity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations.Schema;
using MediatR;

namespace Crypton.Domain.Common.Abstractions;

public interface IDomainEntity
{
    [NotMapped]
    protected ICollection<INotification> ProtectedDomainEvents { get; set; }

    [NotMapped]
    public IReadOnlyCollection<INotification> DomainEvents => this.ProtectedDomainEvents.ToList().AsReadOnly();

    public void AddDomainEvent(INotification domainEvent)
    {
        this.ProtectedDomainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(INotification domainEvent)
    {
        this.ProtectedDomainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        this.ProtectedDomainEvents.Clear();
    }
}