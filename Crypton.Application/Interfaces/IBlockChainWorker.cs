// <copyright file="IBlockChainWorker.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Crypton.Domain.Entities;

namespace Crypton.Application.Interfaces;

public interface IBlockChainWorker
{
    public bool TryEnqueueTransaction(User sender, User receiver, decimal amount);

    public bool TryEnqueueTransaction(User sender, User receiver, Item item);
}