// <copyright file="UserMaterializationInterceptor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Crypton.Application.Interfaces;
using Crypton.Domain.Entities;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Crypton.Infrastructure.Persistence.Interceptors;

public class UserMaterializationInterceptor : IMaterializationInterceptor
{
    private IBlockchainService blockchain = null!;

    public object InitializedInstance(MaterializationInterceptionData materializationData, object instance)
    {
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        this.blockchain ??= materializationData.Context.GetService<IBlockchainService>();

        if (instance is User user)
        {
            user.SetBalance(this.blockchain.GetUserBalance(user.Id));
            user.SetItems(this.blockchain.GetUserItems(user.Id));
        }

        return instance;
    }
}