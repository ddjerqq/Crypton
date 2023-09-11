using ErrorOr;

namespace Crypton.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>
    /// An unbound IErrorOr object that represents a successful operation
    /// </summary>
    public static readonly IErrorOr Success = ErrorOr.ErrorOr.From(0);

    /// <summary>
    /// Create an unbound IErrorOr object from a sequence of errors
    /// </summary>
    public static IErrorOr From(IEnumerable<Error> errors) => ErrorOr<int>.From(errors.ToList());

    /// <summary>
    /// Create an unbound IErrorOr object from a single error
    /// </summary>
    public static IErrorOr From(Error error) => ErrorOr<int>.From(new List<Error> { error });
}