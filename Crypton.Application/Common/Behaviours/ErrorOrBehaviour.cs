// <copyright file="ErrorOrBehaviour.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Crypton.Application.Common.Behaviours;

public sealed class ErrorOrBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    private readonly ILogger<ErrorOrBehaviour<TRequest, TResponse>> logger;

    public ErrorOrBehaviour(ILogger<ErrorOrBehaviour<TRequest, TResponse>> logger)
    {
        this.logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error occured during request {@Request}", request);
            return (dynamic)Error.Failure("uncaught-exception", ex.Message);
        }
    }
}