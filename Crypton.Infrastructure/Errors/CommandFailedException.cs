using ErrorOr;

namespace Crypton.Infrastructure.Errors;

public sealed class CommandFailedException : Exception
{
    public CommandFailedException(IErrorOr error)
    {
        ErrorOr = error;
    }

    public IErrorOr ErrorOr { get; }
}