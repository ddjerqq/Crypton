using ErrorOr;

namespace Crypton.Domain.Common.Errors;

public static partial class Errors
{
    public static class Economy
    {
        public static Error InsufficientFunds = Error.Failure("economy.insufficient_funds", "you do not have enough funds to complete this transaction");
    }
}