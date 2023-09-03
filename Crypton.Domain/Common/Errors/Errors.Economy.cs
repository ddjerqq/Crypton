using ErrorOr;

namespace Crypton.Domain.Common.Errors;

public static partial class Errors
{
    public static class Economy
    {
        public static Error InsufficientFunds = Error.Failure("economy.insufficient_funds", "you do not have enough funds to complete this transaction");
        public static Error InvalidItem = Error.Failure("economy.invalid_item", "the item is invalid");
        public static Error InvalidItemType = Error.Failure("economy.invalid_item_type", "the item type is invalid");
    }
}