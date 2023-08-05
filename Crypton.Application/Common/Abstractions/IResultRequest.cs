// <copyright file="IResultRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using ErrorOr;
using MediatR;

namespace Crypton.Application.Common.Abstractions;

public interface IResultRequest<T> : IRequest<ErrorOr<T>>
{
}