using ErrorOr;
using MediatR;

namespace Crypton.Application.Economy.Commands;

public sealed record CollectDailyCommand : IRequest<ErrorOr<decimal>>;