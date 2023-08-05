// <copyright file="IAppDbContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Crypton.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crypton.Application.Common.Interfaces;

public interface IAppDbContext
{
    public DbSet<User> Users { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default);
}