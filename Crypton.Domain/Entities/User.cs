// <copyright file="User.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Crypton.Domain.Common.Abstractions;

namespace Crypton.Domain.Entities;

public sealed class User : UserBase
{
    public decimal Balance { get; set; }
}