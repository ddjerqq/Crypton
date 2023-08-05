// <copyright file="IResultRequestHandler.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using ErrorOr;
using MediatR;

namespace Crypton.Application.Common.Abstractions;

public interface IResultRequestHandler<in TRequest, TResponse> : IRequestHandler<TRequest, ErrorOr<TResponse>>
    where TRequest : IResultRequest<TResponse>
{
}