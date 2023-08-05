// <copyright file="IgnoreDigitalSignatureAttribute.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Crypton.Infrastructure.Diamond;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class IgnoreDigitalSignatureAttribute : Attribute
{
}