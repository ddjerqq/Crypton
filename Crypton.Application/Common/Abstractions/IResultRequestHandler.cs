using ErrorOr;
using MediatR;

namespace Crypton.Application.Common.Abstractions;

public interface IResultRequestHandler<in TRequest, TResponse> : IRequestHandler<TRequest, ErrorOr<TResponse>>
    where TRequest : IResultRequest<TResponse>
{
}