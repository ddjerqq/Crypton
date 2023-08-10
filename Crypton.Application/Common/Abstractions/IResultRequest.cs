using ErrorOr;
using MediatR;

namespace Crypton.Application.Common.Abstractions;

public interface IResultRequest<T> : IRequest<ErrorOr<T>>
{
}